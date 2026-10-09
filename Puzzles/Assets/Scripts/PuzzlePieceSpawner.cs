using System.Collections.Generic;
using UnityEngine;

public class PuzzlePieceSpawner : MonoBehaviour
{
    [Header("Кусочки паззла")]
    [SerializeField] private Transform piecesParent;

    [Header("Зоны появления")]
    [SerializeField] private Transform leftSpawnZone;
    [SerializeField] private Transform rightSpawnZone;

    [Header("Настройки")]
    [SerializeField] private float piecePadding = 0.1f;
    [SerializeField] private int maxAttempts = 100;

    /*private void Start()
    {
        SpawnPieces();
    }
    */
    public void SpawnPieces()
    {
        // Получаем все кусочки
        List<Transform> pieces = new List<Transform>();

        foreach (Transform piece in piecesParent)
        {
            pieces.Add(piece);
        }

        if (pieces.Count == 0)
        {
            Debug.LogWarning("В Pieces нет кусочков!");
            return;
        }

        // Перемешиваем сам список кусочков
        Shuffle(pieces);

        // Уже занятые области
        List<Bounds> occupiedBounds = new List<Bounds>();

        // Сколько кусочков отправить в левую зону
        int leftCount = Mathf.CeilToInt(pieces.Count / 2f);

        for (int i = 0; i < pieces.Count; i++)
        {
            Transform piece = pieces[i];

            Transform targetZone;

            if (i < leftCount)
            {
                targetZone = leftSpawnZone;
            }
            else
            {
                targetZone = rightSpawnZone;
            }

            TryPlacePiece(piece, targetZone, occupiedBounds);
        }
    }

    private void TryPlacePiece(
        Transform piece,
        Transform spawnZone,
        List<Bounds> occupiedBounds)
    {
        Renderer pieceRenderer = piece.GetComponentInChildren<Renderer>();

        if (pieceRenderer == null)
        {
            Debug.LogWarning(
                $"У {piece.name} нет Renderer. " +
                "Невозможно определить размер кусочка."
            );

            return;
        }

        Vector3 pieceSize = pieceRenderer.bounds.size;

        // Размер зоны берём из Scale объекта
        float zoneWidth = Mathf.Abs(spawnZone.lossyScale.x);
        float zoneHeight = Mathf.Abs(spawnZone.lossyScale.y);

        // Половина размера кусочка
        float halfWidth = pieceSize.x / 2f;
        float halfHeight = pieceSize.y / 2f;

        // Проверяем, помещается ли кусочек вообще
        if (pieceSize.x > zoneWidth ||
            pieceSize.y > zoneHeight)
        {
            Debug.LogWarning(
                $"Кусочек {piece.name} слишком большой " +
                $"для зоны {spawnZone.name}."
            );

            return;
        }

        // Максимальные отступы от центра зоны,
        // чтобы кусочек целиком находился внутри
        float minX = -zoneWidth / 2f + halfWidth;
        float maxX = zoneWidth / 2f - halfWidth;

        float minY = -zoneHeight / 2f + halfHeight;
        float maxY = zoneHeight / 2f - halfHeight;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Случайная позиция внутри зоны
            float randomX = Random.Range(minX, maxX);
            float randomY = Random.Range(minY, maxY);

            Vector3 candidatePosition =
                spawnZone.position +
                new Vector3(randomX, randomY, Random.Range(-0.4f, -0.2f));

            // Временно ставим кусочек в эту позицию,
            // чтобы получить его реальные Bounds
            piece.position = candidatePosition;

            Bounds candidateBounds =
                pieceRenderer.bounds;

            // Добавляем небольшой зазор
            candidateBounds.Expand(piecePadding);

            bool overlaps = false;

            foreach (Bounds occupied in occupiedBounds)
            {
                if (candidateBounds.Intersects(occupied))
                {
                    overlaps = true;
                    break;
                }
            }

            // Если пересечений нет — место подходит
            if (!overlaps)
            {
                occupiedBounds.Add(candidateBounds);
                return;
            }
        }

        Debug.LogWarning(
            $"Не удалось найти свободное место для {piece.name} " +
            $"в зоне {spawnZone.name}."
        );
    }

    private void Shuffle(List<Transform> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            Transform temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    private void OnDrawGizmosSelected()
    {
        DrawZone(leftSpawnZone);
        DrawZone(rightSpawnZone);
    }

    private void DrawZone(Transform zone)
    {
        if (zone == null)
            return;

        Gizmos.matrix = Matrix4x4.TRS(
            zone.position,
            zone.rotation,
            zone.lossyScale
        );

        Gizmos.DrawWireCube(
            Vector3.zero,
            Vector3.one
        );
    }
}