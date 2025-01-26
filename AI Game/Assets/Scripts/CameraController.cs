using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraController : MonoBehaviour
{
    public Camera camera;

    public float zoomSpeed = 1.0f;

    public float minZoom = 3.0f;

    private Vector2 lastWorldPoint;

    private float CameraMinX => camera.transform.position.x - camera.orthographicSize * camera.aspect;
    private float CameraMaxX => camera.transform.position.x + camera.orthographicSize * camera.aspect;
    private float CameraMinY => camera.transform.position.y - camera.orthographicSize;
    private float CameraMaxY => camera.transform.position.y + camera.orthographicSize;


    void Update()
    {
        Vector2 viewPort = camera.ScreenToViewportPoint(Input.mousePosition);

        Vector2 worldPoint = new Vector2(Mathf.Lerp(CameraMinX, CameraMaxX, viewPort.x), Mathf.Lerp(CameraMinY, CameraMaxY, viewPort.y));

        if (Input.GetMouseButtonDown(0))
        {
            lastWorldPoint = worldPoint;
        }
        else if (Input.GetMouseButton(0))
        {
            Vector2 offset = worldPoint - lastWorldPoint;

            camera.transform.Translate(-offset);

            lastWorldPoint = worldPoint - offset;
        }

        // TODO(Steffen): Maybe zoom towards the mouse position

        camera.orthographicSize -= Input.mouseScrollDelta.y * zoomSpeed * Time.deltaTime;

        if(camera.orthographicSize < minZoom)
        {
            camera.orthographicSize = minZoom;
        }

        Bounds bounds = GameManager.instance.walkableTileMap.localBounds;

        if(camera.orthographicSize * camera.aspect > GameManager.instance.walkableTileMap.localBounds.extents.x)
        {
            camera.orthographicSize = GameManager.instance.walkableTileMap.localBounds.extents.x / camera.aspect;
        }

        if (camera.orthographicSize > GameManager.instance.walkableTileMap.localBounds.extents.y)
        {
            camera.orthographicSize = GameManager.instance.walkableTileMap.localBounds.extents.y;
        }

        if (CameraMinX < bounds.min.x)
        {
            float offset = bounds.min.x - CameraMinX;

            camera.transform.Translate(offset, 0.0f, 0.0f);
        }
        else if (CameraMaxX > bounds.max.x)
        {
            float offset = bounds.max.x - CameraMaxX;

            camera.transform.Translate(offset, 0.0f, 0.0f);
        }

        if (CameraMinY < bounds.min.y)
        {
            float offset = bounds.min.y - CameraMinY;

            camera.transform.Translate(0.0f, offset, 0.0f);
        }
        else if (CameraMaxY > bounds.max.y)
        {
            float offset = bounds.max.y - CameraMaxY;

            camera.transform.Translate(0.0f, offset, 0.0f);
        }
    }
}
