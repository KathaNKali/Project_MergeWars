using UnityEngine;
using UnityEngine.UI;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// A class to add on a CharacterPathfinder3D equipped character.
	/// When the assigned UI button is pressed, the character pathfinds to the assigned target.
	/// One-shot: only the first press issues a destination, later presses are ignored.
	/// </summary>
	[AddComponentMenu("MergeWars/Scripts/Movement/ButtonDrivenPathfinderAI3D")]
	public class ButtonDrivenPathfinderAI3D : TopDownMonoBehaviour
	{
		[Header("Setup")]
		/// the UI button that triggers the movement
		[Tooltip("the UI button that triggers the movement")]
		public Button TriggerButton;
		/// the target the character will move to when the button is pressed
		[Tooltip("the target the character will move to when the button is pressed")]
		public Transform Target;

		protected CharacterPathfinder3D _characterPathfinder3D;
		protected bool _destinationSet = false;

		/// <summary>
		/// On awake we grab the pathfinder on this character
		/// </summary>
		protected virtual void Awake()
		{
			_characterPathfinder3D = this.gameObject.GetComponent<CharacterPathfinder3D>();
			if (_characterPathfinder3D == null)
			{
				Debug.LogWarning("[ButtonDrivenPathfinderAI3D] No CharacterPathfinder3D found on " + this.gameObject.name + ".", this);
			}
		}

		/// <summary>
		/// On enable we subscribe to the button's click event
		/// </summary>
		protected virtual void OnEnable()
		{
			if (TriggerButton == null)
			{
				Debug.LogWarning("[ButtonDrivenPathfinderAI3D] No TriggerButton assigned on " + this.gameObject.name + ".", this);
				return;
			}
			TriggerButton.onClick.AddListener(OnButtonPressed);
		}

		/// <summary>
		/// On disable we unsubscribe, symmetrically with OnEnable
		/// </summary>
		protected virtual void OnDisable()
		{
			if (TriggerButton != null)
			{
				TriggerButton.onClick.RemoveListener(OnButtonPressed);
			}
		}

		/// <summary>
		/// When the button is pressed, we send the character to the target (first press only)
		/// </summary>
		protected virtual void OnButtonPressed()
		{
			if (_destinationSet)
			{
				return;
			}

			if (_characterPathfinder3D == null)
			{
				Debug.LogWarning("[ButtonDrivenPathfinderAI3D] Cannot move: no CharacterPathfinder3D.", this);
				return;
			}

			if (Target == null)
			{
				Debug.LogWarning("[ButtonDrivenPathfinderAI3D] Cannot move: no Target assigned.", this);
				return;
			}

			_destinationSet = true;
			_characterPathfinder3D.SetNewDestination(Target);
		}
	}
}
