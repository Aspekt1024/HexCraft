using System;
using System.Collections;
using DefaultNamespace;
using TMPro;
using UI.ControlPanel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Aspekt.Hex.UI
{
    public class TurnIndicator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public interface IEventReceiver
        {
            void OnEndTurnClicked();
            void SetCursorInUI(MonoBehaviour caller, bool isInUI);
        }
        
#pragma warning disable 649
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private ActionsIndicator actionsIndicator;
        [SerializeField] private TextMeshProUGUI actionsCount;
        [SerializeField] private TextMeshProUGUI actionsLabel;
        [SerializeField] private GameObject hourGlassObject;
        [SerializeField] private Button endTurnButton;
#pragma warning restore 649

        private Animator anim;
        private IEventReceiver observer;

        private static readonly int OpeningAnim = Animator.StringToHash("Opening");
        private static readonly int ClosingAnim = Animator.StringToHash("Closing");

        private void Awake()
        {
            anim = GetComponent<Animator>();
            anim.Play(OpeningAnim, 0, 1f);
        }

        public void Init(Tooltip tooltip)
        {
            actionsIndicator.RegisterObserver(tooltip);
        }

        public void RegisterSingleObserver(IEventReceiver observer)
        {
            this.observer = observer;
        }

        public void SetTurn(PlayerData data)
        {
            if (data == null)
            {
                actionsCount.text = "";
                label.text = "";
                return;
            }
            
            if (data.Player.hasAuthority)
            {
                StartCoroutine(ShowOwnTurnRoutine(data));
            }
            else
            {
                StartCoroutine(ShowOpponentTurnRoutine(data));
            }
        }

        public void SetActionCount(PlayerData data, int numActions)
        {
            if (data.Player.hasAuthority)
            {
                actionsCount.text = numActions.ToString();
                actionsLabel.gameObject.SetActive(true);
                actionsIndicator.SetTurnDetails(true, numActions);
            }
            else
            {
                actionsIndicator.SetTurnDetails(false, numActions);
            }
        }

        public void EndTurn()
        {
            observer.OnEndTurnClicked();
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            observer?.SetCursorInUI(this, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            observer?.SetCursorInUI(this, false);
        }

        private IEnumerator ShowOwnTurnRoutine(PlayerData data)
        {
            label.text = "Your turn";
            hourGlassObject.SetActive(false);
            
            // TODO play animation (e.g. flash player turn)
            yield return new WaitForSeconds(1.5f);
            
            label.text = "";
            anim.Play(ClosingAnim, 0, 0f);
            endTurnButton.interactable = true;
        }
        
        private IEnumerator ShowOpponentTurnRoutine(PlayerData data)
        {
            actionsCount.text = "";
            label.text = "";
            hourGlassObject.SetActive(true);
            endTurnButton.interactable = false;
            actionsLabel.gameObject.SetActive(false);
            
            yield return new WaitForSeconds(1.5f);
            anim.Play(OpeningAnim, 0, 0f);
            
            yield return new WaitForSeconds(0.4f);
            label.text = data.Player.DisplayName + "'s Turn";
        }
        
    }
}