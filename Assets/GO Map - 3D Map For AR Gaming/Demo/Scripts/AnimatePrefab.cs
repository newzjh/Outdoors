using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
public class AnimatePrefab : MonoBehaviour {

	public float speed = 500;
	public bool continousRotation;
	bool isAnimating = false;


	async void OnCollisionEnter(Collision collision) {

		print("Animate prefab - on collision enter");
		if (!isAnimating && !continousRotation)
			await rotate(1);
	}

	void OnCollisionStay(Collision collision){
		if (continousRotation)
			transform.Rotate(transform.eulerAngles.x,speed*Time.deltaTime,transform.eulerAngles.z);
	}

	private async UniTask rotate(float time) {

		print("Animate prefab - rotate");
		isAnimating = true;
		float elapsedTime = 0;

		while (elapsedTime < time)
		{
			float value = Mathf.Lerp (0, 360, elapsedTime);
			transform.eulerAngles = new Vector3 (transform.eulerAngles.x, value, transform.eulerAngles.z);
			elapsedTime += Time.deltaTime;
			await UniTask.NextFrame();
		}
		isAnimating = false;
		transform.eulerAngles = new Vector3 (transform.eulerAngles.x, 0, transform.eulerAngles.z);

	}

}
