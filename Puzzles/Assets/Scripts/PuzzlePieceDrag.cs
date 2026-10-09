using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))]
public class PuzzlePieceDrag : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    [Header("Ячейки паззла")]
    [SerializeField] private Transform cellsParent;

    [Header("Настройки прилипания")]
    [SerializeField] private float snapDistance = 1.0f;

    [Header("Камера")]
    [SerializeField] private Camera gameplayCamera;

    // Позиция, в которую кусочек должен вернуться
    private Vector3 returnPosition;

    // Смещение пальца/мыши относительно центра кусочка
    private Vector3 dragOffset;

    // Плоскость, по которой двигается кусочек
    private Plane dragPlane;

    private bool isDragging;


    private void Awake()
    {
        // Если камеру не указали вручную,
        // пытаемся найти основную камеру
        if (gameplayCamera == null)
        {
            gameplayCamera = Camera.main;
        }
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if (gameplayCamera == null)
        {
            Debug.LogError("Не назначена камера для PuzzlePieceDrag!");
            return;
        }

        // Запоминаем текущую позицию.
        // Именно сюда кусочек вернётся,
        // если его отпустят далеко от ячеек.
        returnPosition = transform.position;

        // Создаём плоскость, на которой находится кусочек.
        dragPlane = new Plane(
            gameplayCamera.transform.forward,
            transform.position
        );

        // Получаем мировую позицию пальца/мыши
        if (TryGetPointerWorldPosition(
            eventData.position,
            out Vector3 worldPosition))
        {
            // Запоминаем разницу между положением
            // кусочка и пальца/мыши.
            //
            // Благодаря этому кусочек не прыгает
            // центром под палец.
            dragOffset = transform.position - worldPosition;

            isDragging = true;
        }
    }


    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        if (TryGetPointerWorldPosition(
            eventData.position,
            out Vector3 worldPosition))
        {
            Vector3 newPosition = worldPosition + dragOffset;

            // Сохраняем исходный Z кусочка.
            newPosition.z = returnPosition.z;

            transform.position = newPosition;
        }
    }


    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        isDragging = false;

        Transform nearestCell = FindNearestCell();

        // Если рядом нет ни одной ячейки,
        // возвращаем кусочек туда, откуда его взяли.
        if (nearestCell == null)
        {
            transform.position = returnPosition;
            return;
        }

        float distance = Vector2.Distance(
            transform.position,
            nearestCell.position
        );

        // Если отпустили достаточно близко к ячейке,
        // примагничиваем кусочек к ней.
        if (distance <= snapDistance)
        {
            transform.position = new Vector3(
                nearestCell.position.x,
                nearestCell.position.y,
                returnPosition.z
            );
        }
        else
        {
            // Слишком далеко от ячейки —
            // возвращаем в исходное место.
            transform.position = returnPosition;
        }
    }


    private Transform FindNearestCell()
    {
        if (cellsParent == null)
        {
            Debug.LogError(
                $"Для {gameObject.name} не назначен Cells Parent!"
            );

            return null;
        }

        Transform nearestCell = null;
        float nearestDistance = Mathf.Infinity;

        foreach (Transform cell in cellsParent)
        {
            float distance = Vector2.Distance(
                transform.position,
                cell.position
            );

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestCell = cell;
            }
        }

        return nearestCell;
    }


    private bool TryGetPointerWorldPosition(
        Vector2 screenPosition,
        out Vector3 worldPosition)
    {
        Ray ray = gameplayCamera.ScreenPointToRay(screenPosition);

        if (dragPlane.Raycast(ray, out float enter))
        {
            worldPosition = ray.GetPoint(enter);
            return true;
        }

        worldPosition = Vector3.zero;
        return false;
    }
}