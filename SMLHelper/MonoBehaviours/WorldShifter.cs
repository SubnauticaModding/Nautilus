
// based on the works of Peter Stirling and Tony Lovell
// http://wiki.unity3d.com/index.php/Floating_Origin

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SMLHelper
{

	internal class WorldShifter : MonoBehaviour
	{
		public delegate void ShiftEvent(Vector2 shift);
		public static event ShiftEvent OnWorldShifted;
		public static void Update(int shiftThreshold = 1500, int excludeLayers = 0)
		{
			if (Camera.main == null) return;
			Vector3 camPos = Camera.main.transform.position;

			Vector2 shift = Vector2.zero;
			if (camPos.x < -shiftThreshold) shift = new Vector2(shiftThreshold, 0);
			if (camPos.x > shiftThreshold) shift = new Vector2(-shiftThreshold, 0);
			if (camPos.z < -shiftThreshold) shift = new Vector2(0, shiftThreshold);
			if (camPos.z > shiftThreshold) shift = new Vector2(0, -shiftThreshold);

			if (shift.sqrMagnitude > 1)
			{
				ShiftObjects(shift.x, shift.y);
				ShiftParticles(shift.x, shift.y);
				if (OnWorldShifted != null) OnWorldShifted(shift);
			}
		}

		public static void ShiftObjects(float x, float z, int excludeLayers = 0)
		{
			//in case of moving camera in scene view (or changing pos vars)


			GameObject[] allObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
			for (int i=0; i<allObjects.Length; i++)
			{
				if ((allObjects[i].layer & excludeLayers) != allObjects[i].layer) continue;

				Vector3 oldPos = allObjects[i].transform.position;
				allObjects[i].transform.position = new Vector3(oldPos.x+x, oldPos.y, oldPos.z+z);
}
		}

		public static void ShiftParticles(float x, float z, int excludeLayers = 0)
		{
			ParticleSystem[] allParticles = GameObject.FindObjectsOfType<ParticleSystem>();

			ParticleSystem.Particle[] tempParticles = null;

			for (int i = 0; i < allParticles.Length; i++)
			{
				ParticleSystem particles = allParticles[i];

				if ((particles.gameObject.layer & excludeLayers) != particles.gameObject.layer) continue;

				if (particles.main.simulationSpace != ParticleSystemSimulationSpace.World) continue;

				int maxParticles = particles.main.maxParticles;
				if (maxParticles <= 0) continue;

				//pausing
				bool wasPaused = particles.isPaused;
				bool wasPlaying = particles.isPlaying;
				if (!wasPaused) particles.Pause();

				//shifting particles
				if (tempParticles == null || tempParticles.Length < maxParticles) tempParticles = new ParticleSystem.Particle[maxParticles];

				int numParticles = particles.GetParticles(tempParticles);
				for (int j = 0; j < numParticles; j++)
				{
					Vector3 oldPosition = tempParticles[j].position;
					tempParticles[j].position = new Vector3(oldPosition.x + x, oldPosition.y, oldPosition.z + z);
				}
				particles.SetParticles(tempParticles, numParticles);

				//resuming
				if (wasPlaying) particles.Play();
			}
		}

	}

}//namespace
