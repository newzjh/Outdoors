using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Globalization;
using UnityEngine;
using Unity.Mathematics;
using GoShared;


public class Track
{
    public string name = string.Empty;
    public List<Coordinates> points = new List<Coordinates>();
}


    public class TrackGroup : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
		PassedLength += Time.deltaTime * PassedSpeed;
	}

	//const int nSample = 100;
	const int n = 4;
	public class PathSegment
	{
		public Vector3[] Q;
		public Vector3[] Pts;
		public float[] Us;
		public float Length;
		public float StartLocation;
		public float EndLocation;
		public int nSample = 100;

		public PathSegment(int _nSample)
		{
			nSample = Math.Min(Math.Max(_nSample, 4), 1000);
			Q = new Vector3[4];
			Pts = new Vector3[nSample + 1];
			Us = new float[nSample + 1];
		}
	};

	private List<PathSegment> Segments = new List<PathSegment>();
	[System.NonSerialized]
	public float LengthOfCurve = 0.0f;
	[System.NonSerialized]
	public float PassedLength = 0.0f;
	[System.NonSerialized]
	public float PassedSpeed = 10.0f;
	private static Vector3 CatmullRomPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
	{
		return p1 + (0.5f * (p2 - p0) * t) + 0.5f * (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
				0.5f * (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t;
	}
	private Vector3 GetPositionFromPathByParameter(float srcu, int i)
	{
		float u = srcu;
		Vector3 result = CatmullRomPoint(Segments[i].Q[0], Segments[i].Q[1], Segments[i].Q[2], Segments[i].Q[3], u);
		//Vector3 result = Vector3.Lerp(Segments[i].Q[1], Segments[i].Q[2],u);
		return result;
	}
	public Vector3 GetPositionFromPathByChrod(float u, int i)
	{
		float fValue = (u * Segments[i].nSample);
		float fFloorValue = Mathf.Floor(fValue);
		int nIndexA = (int)fFloorValue;
		int nIndexB = nIndexA + 1;
		if (nIndexB > Segments[i].nSample)
			nIndexB = Segments[i].nSample;
		float fPercentage = fValue - fFloorValue;
		Vector3 v1 = Segments[i].Pts[nIndexA];
		Vector3 v2 = Segments[i].Pts[nIndexB];
		Vector3 v = v1 * (1.0f - fPercentage) + v2 * fPercentage;
		return v;
	}

	public Vector3 GetPositionFromPathByLocation(float location, float u)
	{
		if (location + u < 0.0f)
			location = 0.0f - u;
		if (location + u > LengthOfCurve)
			location = LengthOfCurve - u;
		float fCount = 0.0f;
		for (int i = 0; i < Segments.Count; i++)
		{
			if (location + u >= fCount && location + u < fCount + Segments[i].Length)
			{
				return GetPositionFromPathByChrod((location + u - fCount) / Segments[i].Length, i);
			}
			fCount += Segments[i].Length;
		}
		return GetPositionFromPathByChrod(0.9999f, Segments.Count - 1);
	}

	public Vector3 GetCurrentPosition()
	{
		Vector3 vFishPos = GetPositionFromPathByLocation(PassedLength, 0.0f);
		return vFishPos;
	}

	public void CopmputePathLength()
	{
		LengthOfCurve = 0.0f;
		for (int i = 0; i < Segments.Count; i++)
		{
			Segments[i].StartLocation = LengthOfCurve;
			Segments[i].EndLocation = LengthOfCurve + Segments[i].Length;
			LengthOfCurve += Segments[i].Length;
		}
	}

	public List<LineRenderer> trackRenders = new List<LineRenderer>();
	public List<PathSegment[]> trackPaths = new List<PathSegment[]>();

	public void HighlightAllTrack(bool vis)
	{
		for (int d = 0; d < trackRenders.Count; d++)
			HighlightTrack(d, vis);
	}

	public void HighlightTrack(int index, bool vis)
	{
		var element = trackRenders[index].gameObject.GetComponent<UnityEngine.Rendering.PostProcessing.GlowOutlineElement>();
		if (vis)
		{
			Color c = trackRenders[index].startColor;
			if (!element)
				element = trackRenders[index].gameObject.AddComponent<UnityEngine.Rendering.PostProcessing.GlowOutlineElement>();
			if (element)
				element.color = c;
		}
		else
		{
			if (element)
				DestroyImmediate(element);
		}
	}

	public void ShowTrack(int index)
    {
		Segments.Clear();
		Segments.AddRange(trackPaths[index]);
		CopmputePathLength();
		PassedLength = 0.0f;
		PassedSpeed = 10.0f;

		HighlightAllTrack(false);
		HighlightTrack(index,true);
	}

	public void LoadTracks(List<Track> tracks)
    {
	

		GoMap.GOMap map = GameObject.FindFirstObjectByType<GoMap.GOMap>(FindObjectsInactive.Include);

		List<GameObject> deletelist = new List<GameObject>();
		for(int i=0;i< transform.childCount;i++)
        {
			deletelist.Add(transform.GetChild(i).gameObject);
        }
		foreach(GameObject go in deletelist)
        {
			GameObject.Destroy(go);
        }

		trackRenders.Clear();
		trackPaths.Clear();

		int trackcount = 0;
		foreach(var track in tracks)
		{
			GameObject godemand = new GameObject();
			godemand.name = "track:" + trackcount.ToString();
			trackcount++;
			godemand.transform.parent = transform;
			godemand.transform.localPosition = Vector3.zero;
			godemand.transform.localEulerAngles = Vector3.zero;
			godemand.transform.localScale = Vector3.one;
			godemand.layer = LayerMask.NameToLayer("Track");

			LineRenderer lr = godemand.AddComponent<LineRenderer>();
			lr.alignment = LineAlignment.View;
            Color col = Color.white;
            col.r = UnityEngine.Random.Range(0.2f, 0.8f);
			col.g = 0;// UnityEngine.Random.Range(0.2f, 0.8f);
            col.b = UnityEngine.Random.Range(0.2f, 0.8f);
			Shader s = Shader.Find("Legacy Shaders/Diffuse");
			Material matSolid = new Material(s);
			matSolid.color = col;
			lr.startColor = col;
			lr.endColor = col;
			lr.startWidth = 5.0f;
			lr.endWidth = 5.0f;
			lr.material = matSolid;
			lr.textureMode = LineTextureMode.RepeatPerSegment;
			lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			lr.receiveShadows = false;
			lr.allowOcclusionWhenDynamic = false;
			lr.alignment = LineAlignment.View;
			List<Vector3> pathControlPoints = new List<Vector3>();
			foreach (var v in track.points)
			{
				Vector3 pos = v.convertCoordinateToVector();
				if (map.useElevation)
					pos = GoMap.GOMap.AltitudeToPoint(pos);
				else
					pos.y = 0;
				pos.y += 1.0f;
				pathControlPoints.Add(pos);
			}

			Segments.Clear();
			Vector3 startdir = (pathControlPoints[0] - pathControlPoints[1]).normalized;
			Vector3 enddir = (pathControlPoints[pathControlPoints.Count - 1] - pathControlPoints[pathControlPoints.Count - 2]).normalized;

			List<Vector3> newPathControlPoints = new List<Vector3>();
			newPathControlPoints.Add(pathControlPoints[0] + startdir * 2.0f);
			for (int i = 0; i < pathControlPoints.Count; i++)
			{
				newPathControlPoints.Add(pathControlPoints[i]);
			}
			newPathControlPoints.Add(pathControlPoints[pathControlPoints.Count - 1] + enddir * 2.0f);

			pathControlPoints.Clear();
			pathControlPoints.AddRange(newPathControlPoints.ToArray());


			for (int i = 0; i < pathControlPoints.Count - 3; i++)
			{
				float len = Vector3.Distance(pathControlPoints[i + 1], pathControlPoints[i + 2]) / 6.0f;
				Segments.Add(new PathSegment(Mathf.RoundToInt(len)));
			}

			for (int i = 0; i < Segments.Count; i++)
			{
				Segments[i].Q[0] = pathControlPoints[i + 0];
				Segments[i].Q[1] = pathControlPoints[i + 1];
				Segments[i].Q[2] = pathControlPoints[i + 2];
				Segments[i].Q[3] = pathControlPoints[i + 3];
				float fCurSegmentLen = Vector3.Distance(Segments[i].Q[2], Segments[i].Q[1]);
				if (fCurSegmentLen > 1.0f)
				{
					Vector3 vDir1 = Vector3.Normalize(Segments[i].Q[0] - Segments[i].Q[1]);
					Vector3 vDir2 = Vector3.Normalize(Segments[i].Q[3] - Segments[i].Q[2]);
					Segments[i].Q[0] = Segments[i].Q[1] + fCurSegmentLen * vDir1;
					Segments[i].Q[3] = Segments[i].Q[2] + fCurSegmentLen * vDir2;
				}
			}

			//计算每段路径长度
			for (int i = 0; i < Segments.Count; i++)
			{
				int nSample = Segments[i].nSample;
				float fSample = (float)nSample;
				float fInvSample = 1.0f / fSample;
				float fu = 0.0f;
				//下面用采样积分计算总弧长，保证精确度，偏慢
				Segments[i].Length = 0.0f;
				float[] fChordsByU = new float[nSample + 1];
				Vector3[] vPositionsByU = new Vector3[nSample + 1];
				float[] fLens = new float[nSample + 1];
				fu = 0.0f;
				for (int k = 0; k < nSample; k++)
				{
					Vector3 v1 = GetPositionFromPathByParameter(fu, i);
					Vector3 v2 = GetPositionFromPathByParameter(fu + fInvSample, i);
					fLens[k] = Vector3.Distance(v2, v1);
					fChordsByU[k] = Segments[i].Length;
					vPositionsByU[k] = v1;
					Segments[i].Length += fLens[k];
					fu += fInvSample;
				}
				fChordsByU[nSample] = Segments[i].Length;
				vPositionsByU[nSample] = GetPositionFromPathByParameter(1.0f, i);
				Debug.Assert(Segments[i].Length >= 0.0f && Segments[i].Length <= 99999.0f);

				fu = 0.0f;
				float fCurChord = 0.0f;
				float fDeltaChord = Segments[i].Length / fSample;
				for (int j = 0, k = 0; k < nSample;)
				{
					if (fCurChord >= fChordsByU[j] && fCurChord <= fChordsByU[j + 1])
					{
						float fAlpha = 0.0f;
						if (fChordsByU[j + 1] - fChordsByU[j] > 0.0f)
							fAlpha = (fCurChord - fChordsByU[j]) / (fChordsByU[j + 1] - fChordsByU[j]);
						Segments[i].Pts[k] = vPositionsByU[j] * (1.0f - fAlpha) + vPositionsByU[j + 1] * fAlpha;
						float fu1 = (float)j / fSample;
						float fu2 = (float)(j + 1) / fSample;
						Segments[i].Us[k] = fu1 * (1.0f - fAlpha) + fu2 * fAlpha;
						k++;
						fCurChord += fDeltaChord;
					}
					else
					{
						j++;
					}
				}
				Segments[i].Pts[nSample] = vPositionsByU[nSample];
				Segments[i].Us[nSample] = 1.0f;

				float fOldSegmentLen = Segments[i].Length;
				Segments[i].Length = 0.0f;
				for (int k = 0; k < nSample; k++)
				{
					Vector3 v1 = Segments[i].Pts[k];
					Vector3 v2 = Segments[i].Pts[k + 1];
					float fLen = Vector3.Distance(v2, v1);
					fLens[k] = fLen;
					Segments[i].Length += fLen;
				}
				float fNewSegmentLegnth = Segments[i].Length;
				float fDeltaSegmentLength = Mathf.Abs(fNewSegmentLegnth - fOldSegmentLen);
				Debug.Assert(fDeltaSegmentLength < 100.0f);

			}

			//计算总的路径长度
			CopmputePathLength();

			List<Vector3> curvePositions = new List<Vector3>();
			for (int i = 0; i < Segments.Count; i++)
			{
				int nSample = Segments[i].nSample;
				float fSample = (float)nSample;
				float fInvSample = 1.0f / fSample;
				float fu = 0.0f;

				for (int k = 0; k < Segments[i].Pts.Length; k++)
				{
					Vector3 v = Vector3.Lerp(Segments[i].Q[1], Segments[i].Q[2], fu);
					fu += fInvSample;
                    if (map.useElevation)
                        v = GoMap.GOMap.AltitudeToPoint(v);
                    else
                        v.y = 0;
                    v.y += 1.0f;

                    if (!(i >= 1 && k == 0))
						curvePositions.Add(v);
				}
			}

			lr.positionCount = curvePositions.Count;
			lr.SetPositions(curvePositions.ToArray());
			lr.Simplify(1.0f);

			trackPaths.Add(Segments.ToArray());
			trackRenders.Add(lr);
		}

	}
}
