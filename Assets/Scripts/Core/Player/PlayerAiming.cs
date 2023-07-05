using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAiming : NetworkBehaviour
{
  [SerializeField] private InputReader inputReader;
  [SerializeField] private Transform turretTransform;
}
