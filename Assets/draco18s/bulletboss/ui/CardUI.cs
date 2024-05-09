using System.Collections;
using System.Collections.Generic;
using Assets.draco18s.bulletboss.cards;
using Assets.draco18s.bulletboss.entities;
using Assets.draco18s.bulletboss.pattern;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Assets.draco18s.bulletboss.pattern.timeline.TimelineModuleType;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

namespace Assets.draco18s.bulletboss.ui
{
	public class CardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
	{
		[SerializeField] private Image bgGlint;
		[SerializeField] private Image bg;
		[SerializeField] private Image bgShiny;
		[SerializeField] private Image sprite;
		[SerializeField] private Image spriteShiny;

		[SerializeField] private TextMeshProUGUI cardName;
		[SerializeField] private TextMeshProUGUI cardDesc;
		
		[SerializeField] private Transform hoverTiltCenter;
		[SerializeField] private Transform hoverPosCenter;

		[SerializeField] private Keyframe keyframeParent;
		[SerializeField] private RectTransform keyframeBar;
		[SerializeField] private Button editButton;

		public Card cardRef { get; protected set; }
		private Transform canvasParent;
		private bool isHovered;
		private bool isHeld;
		private Vector3 curRotation;
		private float time;
		private readonly Vector3 origPosition = new Vector3(0,-105,0);
		private Canvas selfCanvas;

		void Awake()
		{
			Transform handTransform = CardHand.instance.transform;
			while (handTransform.GetComponent<Canvas>() == null)
			{
				handTransform = handTransform.parent;
				if (handTransform == null)
					return;
			}
			selfCanvas = transform.GetComponent<Canvas>();
			canvasParent = handTransform;
			time = 1;
			keyframeParent.gameObject.SetActive(false);
		}

		void Update()
		{
			if (isHeld || transform.parent != CardHand.instance.transform)
			{
				keyframeParent.gameObject.SetActive(true);
				hoverTiltCenter.gameObject.SetActive(false);
				if (!isHeld) return;

				Vector3 mouse = Input.mousePosition - new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
				for(int i=0;i<6;i++)
					MoveCard(mouse, 0.5f);

				hoverTiltCenter.localRotation = Quaternion.identity;
				hoverPosCenter.localPosition = Vector3.zero;
				return;
			}
			keyframeParent.gameObject.SetActive(false);
			hoverTiltCenter.gameObject.SetActive(true);
			if (isHovered)
			{
				time -= Time.deltaTime * 2;
				Vector3 mouse = Input.mousePosition - new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
				Vector3 obj = canvasParent.InverseTransformPoint(hoverTiltCenter.position);
				curRotation = (new Vector3(mouse.y - obj.y, (obj.x - mouse.x) * 3, 0)) / 25f;
			}

			hoverTiltCenter.localEulerAngles = Vector3.Lerp(curRotation, Vector3.zero, time * 4);
			hoverPosCenter.localPosition = Vector3.Lerp(origPosition + Vector3.up * 235 + Vector3.back * 75, origPosition, time * 4);

			time += Time.deltaTime;
			time = Mathf.Clamp01(time * 4) / 4;
		}

		private void MoveCard(Vector3 mouse, float amt)
		{
			Vector3 obj = canvasParent.InverseTransformPoint(hoverTiltCenter.position);
			Vector3 dir = new Vector3(mouse.x - obj.x, mouse.y - obj.y, 0);
			while (dir.magnitude > 150)
			{
				dir *= 0.8f;
			}
			transform.Translate(dir * amt * Time.deltaTime);
		}

		public void SetData(Card card)
		{
			cardRef = card;
			bgGlint.color = cardRef.pattern.patternTypeData.rarity.GetColor();
			//bgGlint.color = new Color(bgGlint.color.r, bgGlint.color.g, bgGlint.color.b, 0.75f);
			//bg.color = cardRef.pattern.patternTypeData.rarity.GetColor();
			//bgShiny.color = cardRef.pattern.patternTypeData.rarity.GetColor();
			keyframeParent.SetIcon(card.pattern.patternTypeData);
			cardName.text = cardRef.pattern.patternTypeData.name;
			cardDesc.text = cardRef.pattern.patternTypeData.description;
			float secondWidth = ((RectTransform)TimelineUI.instance.transform).rect.width / 10;
			keyframeBar.sizeDelta = new Vector2(cardRef.pattern.duration * secondWidth + 10, keyframeBar.sizeDelta.y);
			DraggableElement handle = keyframeBar.GetComponentInChildren<DraggableElement>();
			if (cardRef.pattern.patternTypeData.preconfigured)
			{
				handle.Disable();
			}
			else
			{
				handle.Enable();
			}

			sprite.sprite = spriteShiny.sprite = cardRef.pattern.patternTypeData.icon;

			cardRef.pattern.ConfigureKeyframe(keyframeBar, handle, keyframeParent);
			bg.enabled = !card.isUnique;
			bgShiny.enabled = card.isUnique;
			sprite.enabled = !card.isUnique;
			spriteShiny.enabled = card.isUnique;
			editButton.gameObject.SetActive(card.pattern.hasEditableChild);
			editButton.onClick.AddListener(EditChildPattern);
		}

		private void EditChildPattern()
		{
			TimelineUI.instance.SelectChildPattern(cardRef.pattern.childPattern);
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (eventData.pointerPress != null && eventData.pointerPress != this.gameObject) return;
			isHovered = true;
			selfCanvas.overrideSorting = true;
			selfCanvas.sortingOrder = transform.GetSiblingIndex() + 100;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			isHovered = false;
			selfCanvas.overrideSorting = false;
			selfCanvas.sortingOrder = transform.GetSiblingIndex();
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (eventData.pointerCurrentRaycast.gameObject.GetComponent<DraggableHandle>() != null) return;
			if (eventData.pointerCurrentRaycast.gameObject.transform.parent.GetComponent<DraggableHandle>() != null) return;

			List<RaycastResult> list = new List<RaycastResult>();
			EventSystem.current.RaycastAll(eventData, list);
			TimelineUI timeline = null;
			foreach (RaycastResult r in list)
			{
				timeline = r.gameObject.GetComponentInParent<TimelineUI>();
				if (timeline != null)
				{
					break;
				}
			}

			if (timeline != null)
			{
				timeline.RemoveModule(this);
			}
			Vector3 mouse = Input.mousePosition - new Vector3(Screen.width / 2f, Screen.height / 2f, 0);

			transform.SetParent(canvasParent);
			transform.position = canvasParent.TransformPoint(mouse);
			transform.localPosition += (hoverTiltCenter.localPosition)/2;
			//MoveCard(mouse, 0.5f);
			//MoveCard(mouse, 0.5f);
			isHeld = true;
			//Debug.Break();
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (eventData.pointerCurrentRaycast.gameObject?.GetComponent<DraggableHandle>() != null) return;

			isHeld = false;
			List<RaycastResult> list = new List<RaycastResult>();
			EventSystem.current.RaycastAll(eventData, list);

			TimelineUI timeline = null;
			foreach (RaycastResult r in list)
			{
				timeline = r.gameObject.GetComponentInParent<TimelineUI>();
				if (timeline != null)
				{
					break;
				}
			}

			if (timeline!= null)
			{
				timeline.AddModule(this);
			}
			else
			{
				transform.SetParent(CardHand.instance.transform);
			}

			keyframeParent.gameObject.SetActive(transform.parent != CardHand.instance.transform);
			hoverTiltCenter.gameObject.SetActive(transform.parent == CardHand.instance.transform);
		}
	}
}