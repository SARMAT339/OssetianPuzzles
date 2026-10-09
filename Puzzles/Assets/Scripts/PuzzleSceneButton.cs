using UnityEngine;
using UnityEngine.EventSystems;

public class PuzzleSceneButton : MonoBehaviour, IPointerClickHandler
{
    public enum ButtonType
    {
        Play,
        Back,
        Next,
        Restart
    }

    [SerializeField] private PuzzleManager puzzleManager;
    [SerializeField] private ButtonType buttonType;


    public void OnPointerClick(PointerEventData eventData)
    {
        if (puzzleManager == null)
        {
            Debug.LogError(
                $"У кнопки {gameObject.name} " +
                "не назначен PuzzleManager!"
            );

            return;
        }

        switch (buttonType)
        {
            case ButtonType.Play:
                puzzleManager.StartGame();
                break;

            case ButtonType.Back:
                puzzleManager.PreviousPuzzle();
                break;

            case ButtonType.Next:
                puzzleManager.NextPuzzle();
                break;

            case ButtonType.Restart:
                puzzleManager.RestartPuzzle();
                break;
        }
    }
}