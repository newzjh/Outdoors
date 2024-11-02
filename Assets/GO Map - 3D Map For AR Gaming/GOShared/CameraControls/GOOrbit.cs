using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UIElements;

namespace GoShared {

	[AddComponentMenu("Camera-Control/GOOrbit")]
	public class GOOrbit : MonoBehaviour {

		public Transform target;
		public float distance = 55.0f;
		public float orbitSpeed = 1.0f;
		//		public float pinchSpeed = 3.0f;
		[System.NonSerialized]
		public float smoothdis = 55.0f;

		public float yMinLimit = 20f;
		public float yMaxLimit = 60f;

		public float distanceMin = 20f;
		public float distanceMax = 200f;

		public float offset;

		public bool orbitParent;
		Transform objToRotate;

		public AnimationCurve zoomCurve;

		private Rigidbody _rigidbody;

		float prevPinchDist = 0f;

        [HideInInspector] public float currentAngle;

		public bool autoOrbit = false;

		GOClipPlane clipPlane;

        public bool rotateWithHeading = false;

		[System.NonSerialized]
		public bool toggle2D = false;

		// Use this for initialization
		void Start () 
		{

			clipPlane = new GOClipPlane (Camera.main);

			if (orbitParent) {
				objToRotate = transform.parent;
			} else {
				objToRotate = transform;
			}

			_rigidbody = objToRotate.gameObject.GetComponent<Rigidbody>();

			//// Make the rigid body not change rotation
			//if (_rigidbody != null)
			//{
			//	_rigidbody.freezeRotation = true;
			//}

			updateOrbit (true);

			smoothdis = distance;
		}

		private bool bDrag = false;
        private void Update()
        {
            bool condition = (Application.isMobilePlatform && Input.touchCount > 0) || !Application.isMobilePlatform || rotateWithHeading;

            if (target && condition && !GOUtils.IsPointerOverUI())
            {
				if (Input.GetMouseButtonDown(0))
					bDrag = true;
                if (Input.GetMouseButtonUp(0))
                    bDrag = false;

				if (bDrag)
				{
					updateOrbit(false);
				}
            }
        }

        void LateUpdate () 
		{
			//smoothdis = Mathf.MoveTowards(smoothdis, distance, Time.deltaTime * 50.0f);
			smoothdis = Mathf.Lerp(smoothdis, distance, 0.5f);

			float height = EvaluateCurrentHeight(smoothdis);

			Quaternion rotation;
			Vector3 position;
			if (toggle2D)
			{
				rotation = Quaternion.Euler(90, currentAngle, 0);
				Vector3 negDistance = new Vector3(0.0f, 0.0f, -smoothdis);
				position = rotation * negDistance + target.position;
			}
			else
			{
				rotation = Quaternion.Euler(height, currentAngle, 0);
				Vector3 negDistance = new Vector3(0.0f, 0.0f, -smoothdis);
				position = rotation * negDistance + target.position;
			}

			objToRotate.rotation = rotation * Quaternion.Euler(-offset, 0, 0);
			objToRotate.position = position;
		}

		public static float ClampAngle(float angle, float min, float max)
		{
			if (angle < -360F)
				angle += 360F;
			if (angle > 360F)
				angle -= 360F;
			return Mathf.Clamp(angle, min, max);
		}

		private float distanceToAngle ()
		{
			float distanceFactor = (distance / distanceMax);
			float angle = 90 * distanceFactor;

			return angle;
		}

		public void updateOrbit (bool firstLaunch) {

			if (Camera.main == null)
				return;

			bool drag = false;

			Vector3 v1 = Vector3.forward;
			if (Application.isMobilePlatform) {
				drag = Input.touchCount == 1 && Input.GetTouch (0).phase == TouchPhase.Moved;
				if (drag)
					v1 = Input.GetTouch (0).position;
			} else {
				drag = Input.GetMouseButton (0);
				if (drag)
					v1 = Input.mousePosition;
			}

            if (drag || firstLaunch || autoOrbit || rotateWithHeading) {

				if (autoOrbit) {
				
					currentAngle += orbitSpeed;

                } 
				else if (rotateWithHeading) {
                    
                    Input.compass.enabled = true;
                    currentAngle = Input.compass.trueHeading;
                } 
				else
				{
					currentAngle += Input.mousePositionDelta.x * 0.5f * orbitSpeed;
				}
                while (currentAngle < -360F)
                    currentAngle += 360F;
                while (currentAngle > 360F)
                    currentAngle -= 360F;
            } 


			float deltaD = 0;
			if (Application.isMobilePlatform) {
				if (Input.touchCount >= 2) {
					Vector2 touch0, touch1;
					float d;
					touch0 = Input.GetTouch (0).position;
					touch1 = Input.GetTouch (1).position;
					d = Mathf.Abs (Vector2.Distance (touch0, touch1));

					deltaD = Mathf.Clamp (prevPinchDist - d, -1, 1) * (distanceMax - distanceMin) / 25;  //pinchSpeed;
					prevPinchDist = d;

					distance = Mathf.Clamp (distance + deltaD, distanceMin, distanceMax);

				}
			} else {

				deltaD = Input.GetAxis ("Mouse ScrollWheel") * (distanceMax - distanceMin) / 25;
				float newD = distance - deltaD;
				distance = Mathf.Clamp (newD, distanceMin, distanceMax);

			}


			if (clipPlane != null && clipPlane.IsAboutToClip (false)) {
			
				distance += deltaD + 2;
			}



		}

		float EvaluateCurrentHeight (float currentDistance) {
		
			float convValue = (distance- distanceMin) / (distanceMax - distanceMin);
			float factor = zoomCurve.Evaluate (convValue);

			float height = factor *(yMaxLimit-yMinLimit) + yMinLimit;

			return height;

		}



	}

}