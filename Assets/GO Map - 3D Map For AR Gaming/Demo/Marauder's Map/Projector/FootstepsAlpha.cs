using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

[ExecuteInEditMode]
public class FootstepsAlpha : MonoBehaviour {

	Projector proj;
	public float alpha;

	public async void reloadAnimation () {

		await FadeInFoot();
		await FadeOutFoot();
	}

	public async UniTask FadeInFoot () {

		if (proj == null)
			proj = GetComponent<Projector> ();

		await fade (0.3f,true);

	}

	public async UniTask FadeOutFoot (float time = 10.0f) {

		await UniTask.WaitForSeconds(0.5f);
		await fade (time,false);
	}

	private async UniTask fade(float time, bool fadeIn) {

		float elapsedTime = 0;

		while (elapsedTime <= time)
		{
			Color c = proj.material.color;
			if (fadeIn)
				c.a = Mathf.Lerp(0f,1.5f, elapsedTime / time);
			else c.a = Mathf.Lerp(1.0f, -0.5f, elapsedTime / time);
			alpha = c.a;
			proj.material.color = c;
			elapsedTime += Time.deltaTime;

			await UniTask.WaitForEndOfFrame();
		}
	}
		
}
