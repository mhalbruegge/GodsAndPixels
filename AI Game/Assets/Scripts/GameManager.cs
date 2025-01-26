using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Agent;

public class GameManager : MonoBehaviour
{
    //player score
    public int sacrificePoints = 0;
    public event Action<SacrificeType, int, int, Agent> onSacrificeMade = null;
    public int maxGameDuration = 3;
    public Tilemap walkableTileMap;

    public Tilemap collisionTileMap;

    public float timestepIntervall = 1.0f;

    public static GameManager instance;

    public int currentTimestep = 0;

    private bool freezeTime;

    public bool isSimulatingTimestep;

    public List<Agent> agents = new List<Agent>();

    public Prompts agentPrompts;

    SpeechBubble highlighted = null;

    public VFXPlayer thunder;
    public VFXPlayer vulcano;
    public float disasterDuration = 2.0f;

    [Serializable]
    public struct Prompts
    {
        public TextAsset askForConversationPrompt;
        public TextAsset startConversationPrompt;
        public TextAsset continueConversationPrompt;
        public TextAsset disasterPrompt;
        public TextAsset sacrificePrompt;
        public TextAsset choicePrompt;
        public TextAsset gatherSkillPrompt;
    }

    private void Awake()
    {
        instance = this;
        currentTimestep = 0;
    }

    void GameOver()
    {
        Debug.LogError("TODO");
        GodManager.instance.highscore = GameManager.instance.sacrificePoints;
        GodManager.instance.LoadScene("GameOver");
    }

    private void HandleMovement()
    {
        if (Input.GetKey(KeyCode.W))
        {
            agents[0].Move(Vector2Int.up);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            agents[0].Move(Vector2Int.down);
        }
        else if (Input.GetKey(KeyCode.A))
        {
            agents[0].Move(Vector2Int.left);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            agents[0].Move(Vector2Int.right);
        }

    }

    void Update()
    {
        HandleMovement();

        //if (Input.GetKeyDown(KeyCode.N))
        //{
        //    freezeTime = !freezeTime;
        //}

        //if (isSimulatingTimestep)
        //{
        //    return;
        //}

        //if (!freezeTime)
        //{
        //    currentTimestep += Time.deltaTime;

        //    if (currentTimestep > timestepIntervall)
        //    {
        //        _ = SimulateTimestep();

        //        currentTimestep = 0.0f;
        //    }
        //}
    }

    public enum SacrificeType
    {
        Goat,
        Vegetables,
        Boar,
        Fish
    }

    Dictionary<SacrificeType, int> sacrificeValueMap = new Dictionary<SacrificeType, int>()
    {
        { SacrificeType.Goat, 3 },
        { SacrificeType.Vegetables, 1 },
        { SacrificeType.Boar, 5 },
        { SacrificeType.Fish, 2 },
    };

    public float talkDistance = 3.0f;

    public void DoSacrifice(SacrificeType type, int amount, Agent agent)
    {
        sacrificePoints += sacrificeValueMap[type] * amount;
        onSacrificeMade?.Invoke(type, amount, sacrificeValueMap[type], agent);
    }
    public event Action onSimulationStarted = null;
    public event Action onSimulationOver = null;
    public async Task SimulateTimestep()
    {
        if (isSimulatingTimestep)
            return;

        //GAMEOVER LOGIC
        currentTimestep += 1;
        if (currentTimestep > maxGameDuration)
        {
            GameOver();
        }

        bool oneAgentAlive = false;
        foreach (Agent agent in agents)
        {
            if (agent.isAlive)
            {
                oneAgentAlive = true;
                break;
            }
        }

        if (!oneAgentAlive)
        {
            GameOver();
        }

        //SIM LOGIC
        onSimulationStarted?.Invoke();
        isSimulatingTimestep = true;

        Debug.Log("SimulateTimestep");

        foreach (Agent agent in agents)
        {
            await agent.SimulateTimestep();
        }

        isSimulatingTimestep = false;
        onSimulationOver?.Invoke();
    }

    public void RegisterAgent(Agent agent)
    {
        agents.Add(agent);
    }

    public void HighlightChatBubble(SpeechBubble speechBubble)
    {
        const int highlightLayer = 22;
        const int defaultLayer = 20;

        if (highlighted != null)
        {
            highlighted.SetRenderingLayer(defaultLayer);
        }

        highlighted = speechBubble;

        highlighted.SetRenderingLayer(highlightLayer);
    }

    public enum DisasterType
    {
        None,
        Thunder,
        Vulcano
    }

    public void Disaster(DisasterType type, string prompt)
    {
        switch (type)
        {
            case DisasterType.Thunder:
                thunder.Play(disasterDuration);
                break;
            case DisasterType.Vulcano:
                vulcano.Play(disasterDuration);
                break;
            default:
                break;
        }

        foreach (Agent agent in agents)
        {
            agent.DisasterEvent(prompt);
        }
    }

    //public void ThunderDisaster()
    //{
    //    thunder.Play(disasterDuration);

    //    foreach (Agent agent in agents)
    //    {
    //        agent.DisasterEvent("A epic thunderstorms floods the village, the gods are angry");
    //    }
    //}

    //public void VulcanoDisaster()
    //{
    //    vulcano.Play(disasterDuration);

    //    foreach (Agent agent in agents)
    //    {
    //        agent.DisasterEvent("A vulcano outbreak happened, the gods are angry");
    //    }
    //}
}
