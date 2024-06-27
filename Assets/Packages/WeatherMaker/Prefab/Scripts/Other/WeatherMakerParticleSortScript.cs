using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalRuby.WeatherMaker
{
    /// <summary>
    /// Particle system sorting script (slow)
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class WeatherMakerParticleSortScript : MonoBehaviour
    {
        private ParticleSystem particles;
        private ParticleSystem.Particle[] particlesArray = new ParticleSystem.Particle[4096];
        private List<float> particleDistances = new List<float>();

        private void swap(ParticleSystem.Particle[] list, int a, int b)
        {
            ParticleSystem.Particle temp = list[a];
            list[a] = list[b];
            list[b] = temp;
        }

        private void swap(List<float> list, int a, int b)
        {
            float temp = list[a];
            list[a] = list[b];
            list[b] = temp;
        }

        private int partition(ParticleSystem.Particle[] list, int left, int right, int pivotidx)
        {
            float pivotDistance = particleDistances[pivotidx];
            swap(list, right, pivotidx);
            swap(particleDistances, right, pivotidx);
            int compare;

            for (int i = left; i < right; i++)
            {
                compare = particleDistances[i].CompareTo(pivotDistance);
                if (compare > 0)
                {
                    swap(list, i, left);
                    swap(particleDistances, i, left);
                    left++;
                }
            }

            swap(list, left, right);
            swap(particleDistances, left, right);
            return left;
        }

        private void _quickSort(ParticleSystem.Particle[] list, int left, int right)
        {
            if (right > left)
            {
                int pivotidx = left;
                pivotidx = partition(list, left, right, pivotidx);
                _quickSort(list, left, pivotidx);
                _quickSort(list, pivotidx + 1, right);
            }
        }

        private void Swap(ParticleSystem.Particle[] array, int index1, int index2)
        {
            ParticleSystem.Particle tmp = array[index1];
            array[index1] = array[index2];
            array[index2] = tmp;
        }

        private void QuickSort(ParticleSystem.Particle[] array, int startIndex, int endIndex)
        {
            if (startIndex >= endIndex)
            {
                return;
            }

            int middleIndex = Partition(array, startIndex, endIndex);
            QuickSort(array, startIndex, middleIndex - 1);
            QuickSort(array, middleIndex + 1, endIndex);
        }

        private int Partition(ParticleSystem.Particle[] array, int startIndex, int endIndex)
        {
            int middle = startIndex;
            int compare;
            for (int u = startIndex; u <= endIndex - 1; u++)
            {
                compare = particleDistances[u].CompareTo(particleDistances[endIndex]);
                if (compare <= 0)
                {
                    Swap(array, u, middle);
                    middle++;
                }
            }
            Swap(array, middle, endIndex);
            return middle;
        }

        private void OnEnable()
        {
            if (WeatherMakerCommandBufferManagerScript.Instance != null)
            {
                WeatherMakerCommandBufferManagerScript.Instance.RegisterPreCull(CameraPreCull, this);
            }
            particles = GetComponent<ParticleSystem>();
        }

        private void OnDisable()
        {
            if (WeatherMakerCommandBufferManagerScript.Instance != null)
            {
                WeatherMakerCommandBufferManagerScript.Instance.UnregisterPreCull(this);
            }
        }

        private void CameraPreCull(Camera camera)
        {
            int size = particles.GetParticles(particlesArray);
            Vector3 camPos = camera.transform.position;
            for (int i = 0; i < size; i++)
            {
                Vector3 center = particlesArray[i].position;
                particleDistances.Add(Vector3.SqrMagnitude(center - camPos));
            }
            _quickSort(particlesArray, 0, size - 1);
            particles.SetParticles(particlesArray, size);
            particleDistances.Clear();
        }
    }
}
