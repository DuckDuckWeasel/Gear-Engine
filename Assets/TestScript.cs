using GearEngine.CarSimulation.Entity;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    public CarEntity Entity => entity;

    [SerializeField]
    private CarEntity entity;
}
