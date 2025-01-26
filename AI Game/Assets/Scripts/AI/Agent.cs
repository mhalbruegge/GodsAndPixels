using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Agent : MonoBehaviour
{
    public AgentLocation sacrificeLocation;
    public AgentLocation skillLocation;
    public AgentLocation socializeLocation;

    private AgentLocation currentLocation;

    public GameManager.SacrificeType inventoryItem;
    public int inventoryAmount = 1;
    public int healthPoints = 3;
    public string skillDescription = "";
    public Image inventoryIcon = null;
    public bool isAlive => healthPoints > 0;
    public enum ActionType
    {
        Talk,
        Socialize,
        Sacrifice,
        Skill,
        Idle
    }

    public AICharacter aiCharacter;
    public Animator animator;

    string currentDisaster = "";

    public Vector2Int tilePosition;

    public Agent inConversationWith;
    public AudioSource audioSource;
    public AudioClip dieClip;

    public GameObject inventoryBubble;

    public GameObject[] lifeIndicatorFull;
    public GameObject[] lifeIndicatorEmpty;

    public bool isNextInConversation;

    public string lastMessage;

    private Room currentRoom;

    public event Action<string> OnAgentSaysSomething;
    public event Action OnAgentFinishedPath;

    public struct ConversationAnswer
    {
        public string message;
        public bool quitConversation;
    }

    public struct SacrificeAnswer
    {
        public string message;
        public int amount;
    }

    public struct DisasterAnswer
    {
        public string message;
        public float looseHealthChance;
    }

    private void Start()
    {
        GameManager.instance.RegisterAgent(this);
        ChangeInventory(0);

        currentRoom = GetComponentInParent<Room>();

        tilePosition = new Vector2Int((int)transform.position.x, (int)transform.position.y - 1);

        OnAgentSaysSomething?.Invoke("");
    }

    private float movementStepTime = 0.5f;

    public bool isMoving;
    public bool isNavigating;
    public bool shouldQuitNavigation;

    private IEnumerator Move(Vector2Int from, Vector2Int to)
    {
        isMoving = true;

        from.y += 1;
        to.y += 1;

        float currentStepTime = 0.0f;

        while (currentStepTime < movementStepTime)
        {
            float percentage = currentStepTime / movementStepTime;
            Vector2 targetPosition = Vector2.Lerp(from, to, percentage);

            transform.position = targetPosition;

            currentStepTime += Time.deltaTime;

            yield return null;
        }

        transform.position = new Vector3(to.x, to.y, 0.0f);

        animator.ForceStateNormalizedTime(0.0f);
        animator.speed = 0.0f;

        isMoving = false;
    }

    public IEnumerator MoveAlongPath(List<Vector2Int> path)
    {
        isNavigating = true;

        int nextIndex = 0;
        while (nextIndex != path.Count)
        {
            while (isMoving)
            {
                yield return null;
            }

            if (shouldQuitNavigation)
            {
                path.Clear();
                isNavigating = false;
                shouldQuitNavigation = false;
                yield break;
            }

            Vector2Int nextTarget = path[nextIndex++];

            Move(nextTarget - tilePosition);
        }

        while (isMoving)
        {
            yield return null;
        }

        OnAgentFinishedPath?.Invoke();

        isNavigating = false;
    }

    public void Move(Vector2Int movement)
    {
        if (isMoving)
        {
            return;
        }

        Vector2Int newPosition = tilePosition + movement;

        if (GameManager.instance.collisionTileMap.GetTile((Vector3Int)newPosition) != null)
        {
            return;
        }

        if (GameManager.instance.walkableTileMap.GetTile((Vector3Int)newPosition) == null)
        {
            return;
        }

        foreach (Agent agent in GameManager.instance.agents)
        {
            if (agent.tilePosition == newPosition)
            {
                return;
            }
        }

        animator.speed = 1.0f;

        if (movement == Vector2Int.up)
        {
            //animator.SetInteger("State", (int)AnimationState.Up);

            animator.Play("Up"); ;
        }
        else if (movement == Vector2Int.down)
        {
            //animator.SetInteger("State", (int)AnimationState.Down);

            animator.Play("Down"); ;
        }
        else if (movement == Vector2Int.left)
        {
            //animator.SetInteger("State", (int)AnimationState.Left);

            animator.Play("Left"); ;
        }
        else if (movement == Vector2Int.right)
        {
            //animator.SetInteger("State", (int)AnimationState.Right);

            animator.Play("Right");

            animator.Play("Right", 0, 0.0f);
        }

        StartCoroutine(Move(tilePosition, newPosition));

        tilePosition = newPosition;
    }

    public enum AnimationState : int
    {
        Idle = 0,
        Up = 1,
        Down = 2,
        Right = 3,
        Left = 4
    }

    public async Task SimulateTimestep()
    {
        if (!isAlive)
        {
            OnAgentSaysSomething?.Invoke("(x_x;)");
            return;
        }

        OnAgentSaysSomething?.Invoke("");

        if (!string.IsNullOrEmpty(currentDisaster))
        {
            inConversationWith = null;
            await HandleDisaster();
            return;
        }

        if (isNavigating)
        {
            return;
        }

        if (inConversationWith != null)
        {
            if (isNextInConversation)
            {
                await ContinueConversation(inConversationWith);
            }
        }
        else
        {
            ActionType action = await HandleChoice();
            switch (action)
            {
                case ActionType.Talk:
                    await HandleTalk();
                    break;
                case ActionType.Socialize:
                    await HandleSocialize();
                    break;
                case ActionType.Sacrifice:
                    await HandleSacrifice();
                    break;
                case ActionType.Skill:
                    await HandleSkill();
                    break;
                case ActionType.Idle:
                    Debug.LogWarning(name + " agent propably failed making a choice");
                    await HandleIdle();
                    break;
                default:
                    break;
            }

            await Task.Delay(2000);
        }
    }

    async Task HandleTalk()
    {
        foreach (Agent other in TalkableAgents())
        {
            if (other == this)
            {
                continue;
            }

            if (!other.isAlive)
            {
                continue;
            }

            if (other.inConversationWith != null)
            {
                continue;
            }

            bool startInteraction = await ShouldStartInteraction(other);

            Debug.Log(aiCharacter.playerName + " want to talk with " + other.aiCharacter.playerName + ": " + startInteraction);

            if (startInteraction)
            {
                await StartConversation(other);

                break;
            }
        }
    }

    private async Task<bool> ShouldStartInteraction(Agent other)
    {
        string prompt = GameManager.instance.agentPrompts.askForConversationPrompt.text;
        prompt = prompt.Replace("{agent_name}", other.aiCharacter.playerName);

        string answer = await aiCharacter.Chat(prompt);

        switch (answer.ToLower())
        {
            case "yes":
            case "yes.":
                return true;
            case "no":
            case "no.":
                return false;
            default: Debug.LogError("Unexpected agent answer: " + answer); return false;
        }
    }

    private async Task StartConversation(Agent other)
    {
        string prompt = GameManager.instance.agentPrompts.startConversationPrompt.text;
        prompt = prompt.Replace("{agent_name}", other.aiCharacter.playerName);

        string answer = await aiCharacter.Chat(prompt);

        Debug.Log(aiCharacter.playerName + " says: " + answer);
        OnAgentSaysSomething?.Invoke(answer);

        lastMessage = answer;

        inConversationWith = other;
        other.inConversationWith = this;
        other.isNextInConversation = true;
    }

    private async Task<bool> ContinueConversation(Agent other)
    {
        string prompt = GameManager.instance.agentPrompts.continueConversationPrompt.text;
        prompt = prompt.Replace("{agent_name}", other.aiCharacter.playerName);
        prompt = prompt.Replace("{agent_message}", other.lastMessage);

        string reply = await aiCharacter.Chat(prompt);

        ConversationAnswer answer = JsonUtility.FromJson<ConversationAnswer>(reply);

        Debug.Log(aiCharacter.playerName + " says: " + reply);
        Debug.Log(aiCharacter.playerName + " wants to quit the conversation: " + answer.quitConversation);

        OnAgentSaysSomething?.Invoke(answer.message);
        lastMessage = answer.message;

        if (answer.quitConversation)
        {
            inConversationWith = null;
            other.inConversationWith = null;
            ChangeHealth(1);
        }
        else
        {
            isNextInConversation = false;
            other.isNextInConversation = true;
        }

        return answer.quitConversation;
    }

    private void OnMouseDown()
    {
        // your game function
        // string message = "What's your name?";
        // message = "You experienced: A lightning strike hit you!\r\n\r\nWrite me an JSON in the following format:\r\n\r\n{\r\n    \"importance\": <integer_number_here>\r\n}\r\n\r\nwhere you you replace <integer_number_here> with your perceived importance of the event.";
        // _ = aiCharacter.Chat(message, HandleReply);
    }

    private void OnMouseEnter()
    {
        inventoryBubble.SetActive(true);
    }

    private void OnMouseExit()
    {
        inventoryBubble.SetActive(false);
    }

    public void DisasterEvent(string prompt)
    {
        currentDisaster = prompt;
    }

    public void ChangeHealth(int amount)
    {
        healthPoints += amount;
        healthPoints = Mathf.Clamp(healthPoints, 0, 3);

        for (int i = 0; i < 3; ++i)
        {
            bool isActive = i < healthPoints;

            lifeIndicatorFull[i].SetActive(isActive);
            lifeIndicatorEmpty[i].SetActive(!isActive);
        }

        if (!isAlive)
        {
            audioSource.PlayOneShot(dieClip);
            //gameObject.SetActive(false);
            transform.rotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(80, 100));
        }
    }

    public void ChangeInventory(int amount)
    {
        inventoryAmount += amount;
        inventoryBubble.GetComponentInChildren<TextMeshProUGUI>().text = inventoryAmount.ToString();
    }

    public async Task HandleDisaster()
    {
        if (isNavigating)
        {
            shouldQuitNavigation = true;

            if(currentLocation != null)
            {
                currentLocation.currentAgent = null;
            }
            currentLocation = null;

            OnAgentFinishedPath = null;
        }

        currentDisaster = null;

        string prompt = GameManager.instance.agentPrompts.disasterPrompt.text;
        prompt = prompt.Replace("{disaster_description}", currentDisaster);

        string reply = await aiCharacter.Chat(prompt);
        DisasterAnswer answer = JsonUtility.FromJson<DisasterAnswer>(reply);

        Debug.Log(aiCharacter.playerName + " says: " + reply);
        OnAgentSaysSomething?.Invoke(answer.message);
        lastMessage = answer.message;

        if (UnityEngine.Random.value > answer.looseHealthChance)
            ChangeHealth(-1);
    }

    public async Task HandleIdle()
    {
        OnAgentSaysSomething?.Invoke("... zzz ...");

        await Task.CompletedTask;
    }

    struct GatherSkill
    {
        public int findItemAmount;
        public string message;
    }

    public async Task HandleSkill()
    {
        if (currentLocation != null)
        {
            currentLocation.currentAgent = null;
        }

        currentLocation = skillLocation;
        currentLocation.currentAgent = this;

        Vector2Int targetPosition = new Vector2Int((int)skillLocation.transform.position.x, (int)skillLocation.transform.position.y);

        if(tilePosition != targetPosition)
        {
            AgentNavigator.instance.MoveToPosition(this, targetPosition);

            OnAgentFinishedPath += HandleSkillAtLocation;
        }
        else
        {
            HandleSkillAtLocation();
        }
    }

    private async void HandleSkillAtLocation()
    {
        animator.Play(skillLocation.finalAnimationState.ToString(), 0, 0.0f);

        string prompt = GameManager.instance.agentPrompts.gatherSkillPrompt.text;
        prompt = prompt.Replace("{inventory_item}", inventoryItem.ToString());
        prompt = prompt.Replace("{skill_description}", skillDescription);

        string reply = await aiCharacter.Chat(prompt);
        GatherSkill answer = JsonUtility.FromJson<GatherSkill>(reply);

        ChangeInventory(answer.findItemAmount);

        Debug.Log(aiCharacter.playerName + " says: " + reply);
        OnAgentSaysSomething?.Invoke(answer.message);
        lastMessage = answer.message;

        OnAgentFinishedPath -= HandleSkillAtLocation;
    }

    public async Task HandleSocialize()
    {
        if (currentLocation != null)
        {
            currentLocation.currentAgent = null;
        }

        currentLocation = socializeLocation;
        currentLocation.currentAgent = this;

        Vector2Int targetPosition = new Vector2Int((int)socializeLocation.transform.position.x, (int)socializeLocation.transform.position.y);

        if (tilePosition != targetPosition)
        {
            AgentNavigator.instance.MoveToPosition(this, targetPosition);
        }
    }

    public async Task HandleSacrifice()
    {
        //if (currentLocation != null)
        //{
        //    currentLocation.currentAgent = null;
        //}

        //currentLocation = sacrificeLocation;
        //currentLocation.currentAgent = this;

        Vector2Int targetPosition = new Vector2Int((int)sacrificeLocation.transform.position.x, (int)sacrificeLocation.transform.position.y);

        if (tilePosition != targetPosition)
        {
            AgentNavigator.instance.MoveToPosition(this, targetPosition);

            OnAgentFinishedPath += HandleSacrificeAtLocation;
        }
        else
        {
            HandleSacrificeAtLocation();
        }
    }

    private async void HandleSacrificeAtLocation()
    {
        animator.Play(sacrificeLocation.finalAnimationState.ToString(), 0, 0.0f);

        string prompt = GameManager.instance.agentPrompts.sacrificePrompt.text;
        prompt = prompt.Replace("{available_sacrifice_items}", inventoryItem.ToString() + " = " + inventoryAmount);
        prompt = prompt.Replace("{current_healthpoints}", healthPoints + " / 3");

        string reply = await aiCharacter.Chat(prompt);

        SacrificeAnswer answer = JsonUtility.FromJson<SacrificeAnswer>(reply);

        Debug.Log(aiCharacter.playerName + " says: " + reply);
        OnAgentSaysSomething?.Invoke(answer.message);
        if (answer.amount > 0)
            GameManager.instance.DoSacrifice(inventoryItem, answer.amount, this);
        ChangeInventory(-answer.amount);

        lastMessage = answer.message;

        OnAgentFinishedPath -= HandleSacrificeAtLocation;
    }

    List<Agent> TalkableAgents()
    {
        List<Agent> talkable = new List<Agent>();

        foreach (Agent agent in GameManager.instance.agents)
        {
            if (agent == this)
                continue;

            if (Vector3.Distance(agent.transform.position, transform.position) > GameManager.instance.talkDistance)
                continue;

            if (agent.inConversationWith != null)
                continue;

            talkable.Add(agent);
        }

        return talkable;
    }

    string FirstCharUpper(string str)
    {
        return char.ToUpper(str[0]) + str.Substring(1);
    }

    public async Task<ActionType> HandleChoice()
    {
        string prompt = GameManager.instance.agentPrompts.choicePrompt.text;

        string options = "";
        string notAvaliableOptions = "";

        Vector2Int socializeTileLocation = new Vector2Int((int)socializeLocation.transform.position.x, (int)socializeLocation.transform.position.y);

        if (tilePosition != socializeTileLocation)
        {
            options += "Socialize\n";
        }
        else
        {
            notAvaliableOptions += "Socialize: already in area\n";
        }

        List<Agent> talkables = TalkableAgents();
        if (talkables.Count > 0)
        {
            options += "Talk\n";
        }
        else
        {
            notAvaliableOptions += "Talk: no near characters\n";
        }
        if (inventoryAmount > 0)
        {
            options += "Sacrifice\n";
        }
        else if(inventoryAmount == 0)
        {
            notAvaliableOptions += "Sacrifice: no items left\n";
        }

        if (!string.IsNullOrEmpty(skillDescription))
        {
            options += "Skill\n";
        }
        else
        {
            notAvaliableOptions += "Skill: your character has no skill avaliable\n";
        }

        prompt = prompt.Replace("{avaliable_options}", options);
        prompt = prompt.Replace("{not_avaliable_options}", notAvaliableOptions);
        prompt = prompt.Replace("{current_inventory}", inventoryItem + " = " + inventoryAmount);
        prompt = prompt.Replace("{current_health}", healthPoints + " / 3");
        prompt = prompt.Replace("{agent_skill_description}", skillDescription);
        string choice = await aiCharacter.Chat(prompt);
        Debug.Log(aiCharacter.playerName + " chooses to: " + choice);

        if (!Enum.TryParse(FirstCharUpper(choice).Replace(".", ""), out ActionType result))
        {
            result = ActionType.Idle;
        }

        return result;
    }
}
