using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [Header("Очередь паззлов")]
    [SerializeField] private Transform puzzlesParent;

    [Header("Кнопки-спрайты")]
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject backButton;
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject restartButton;

    private List<GameObject> puzzles = new List<GameObject>();

    private int currentPuzzleIndex = -1;
    private bool gameStarted = false;


    private void Start()
    {
        // Получаем все паззлы из родительского объекта
        foreach (Transform puzzle in puzzlesParent)
        {
            puzzles.Add(puzzle.gameObject);
        }

        if (puzzles.Count == 0)
        {
            Debug.LogError("В Puzzles нет паззлов!");
            return;
        }

        // Все паззлы скрыты при запуске
        foreach (GameObject puzzle in puzzles)
        {
            puzzle.SetActive(false);
        }

        // В начале видна только кнопка "Играть"
        playButton.SetActive(true);
        backButton.SetActive(false);
        nextButton.SetActive(false);
        restartButton.SetActive(false);
    }


    // ---------------------------------------------------------
    // КНОПКА "ИГРАТЬ"
    // ---------------------------------------------------------

    public void StartGame()
    {
        gameStarted = true;

        // Открываем первый паззл
        currentPuzzleIndex = 0;

        OpenPuzzle(currentPuzzleIndex);

        // Убираем кнопку "Играть"
        playButton.SetActive(false);

        // Показываем остальные
        backButton.SetActive(true);
        nextButton.SetActive(true);
        restartButton.SetActive(true);

        UpdateNavigationButtons();
    }


    // ---------------------------------------------------------
    // ОТКРЫТИЕ ПАЗЗЛА
    // ---------------------------------------------------------

    private void OpenPuzzle(int index)
    {
        // Выключаем все паззлы
        foreach (GameObject puzzle in puzzles)
        {
            puzzle.SetActive(false);
        }

        // Включаем выбранный паззл
        GameObject currentPuzzle = puzzles[index];
        currentPuzzle.SetActive(true);

        // Находим его Spawner
        PuzzlePieceSpawner spawner =
            currentPuzzle.GetComponent<PuzzlePieceSpawner>();

        if (spawner == null)
        {
            Debug.LogError(
                $"У {currentPuzzle.name} нет PuzzlePieceSpawner!"
            );

            return;
        }

        // Разбрасываем кусочки
        spawner.SpawnPieces();

        UpdateNavigationButtons();
    }


    // ---------------------------------------------------------
    // КНОПКА "НАЗАД"
    // ---------------------------------------------------------

    public void PreviousPuzzle()
    {
        if (!gameStarted)
            return;

        if (currentPuzzleIndex <= 0)
            return;

        currentPuzzleIndex--;

        OpenPuzzle(currentPuzzleIndex);
    }


    // ---------------------------------------------------------
    // КНОПКА "ДАЛЕЕ"
    // ---------------------------------------------------------

    public void NextPuzzle()
    {
        if (!gameStarted)
            return;

        if (currentPuzzleIndex >= puzzles.Count - 1)
            return;

        currentPuzzleIndex++;

        OpenPuzzle(currentPuzzleIndex);
    }


    // ---------------------------------------------------------
    // КНОПКА "ЗАНОВО"
    // ---------------------------------------------------------

    public void RestartPuzzle()
    {
        if (!gameStarted)
            return;

        PuzzlePieceSpawner spawner =
            puzzles[currentPuzzleIndex]
            .GetComponent<PuzzlePieceSpawner>();

        if (spawner == null)
        {
            Debug.LogError(
                $"У {puzzles[currentPuzzleIndex].name} " +
                "нет PuzzlePieceSpawner!"
            );

            return;
        }

        // Снова перемешиваем текущий паззл
        spawner.SpawnPieces();
    }


    // ---------------------------------------------------------
    // УПРАВЛЕНИЕ КНОПКАМИ НАЗАД / ДАЛЕЕ
    // ---------------------------------------------------------

    private void UpdateNavigationButtons()
    {
        // Если первый паззл — кнопку "Назад" скрываем
        if (currentPuzzleIndex == 0)
        {
            backButton.SetActive(false);
        }
        else
        {
            backButton.SetActive(true);
        }

        // Если последний паззл — кнопку "Далее" скрываем
        if (currentPuzzleIndex == puzzles.Count - 1)
        {
            nextButton.SetActive(false);
        }
        else
        {
            nextButton.SetActive(true);
        }
    }
}