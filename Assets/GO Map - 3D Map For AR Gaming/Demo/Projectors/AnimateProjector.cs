using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class AnimateProjector : MonoBehaviour {

	Projector projector;
	public bool continuous = false;
	public float minSize = 0f;
	public float maxSize = 10f;
	public float animationTime = 0.3f;
	public float interval = 0f;

	// Use this for initialization
	async void Start () {
		
		projector = GetComponent<Projector> ();

		await Animate(animationTime);

	}

	private async UniTask Animate(float time) {

		float size = maxSize;

		if (!continuous)
			size *= Vector3.Distance (Camera.main.transform.position, transform.position) / 200;

		float elapsedTime = 0;
		while (elapsedTime < time)
		{
            if (this == null)
                return;

            float t = (elapsedTime / time);

			projector.orthographicSize = Mathf.Lerp(minSize, size, Mathf.SmoothStep(0, 1.0f, t)
				);
			elapsedTime += Time.deltaTime;
			await UniTask.WaitForEndOfFrame();
		}

		elapsedTime = 0;
		while (elapsedTime < time)
		{
            if (this == null)
                return;

            float t = (elapsedTime / time);

			projector.orthographicSize = Mathf.Lerp(size, minSize, Mathf.SmoothStep(0, 1.0f, t)
				);
			elapsedTime += Time.deltaTime;
			await UniTask.WaitForEndOfFrame();
		}

		if (this == null)
			return;

		if (continuous)
		{
			await UniTask.WaitForSeconds(interval);
            if (this == null)
                return;
            await Animate(animationTime);
		}
		else
		{
			GameObject.Destroy(gameObject);
		}
	}

}
