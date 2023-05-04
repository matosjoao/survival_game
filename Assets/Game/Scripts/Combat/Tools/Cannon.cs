using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour, IInteractable
{
    [Header("Fire Properties")]
    [SerializeField] private GameObject bullet;
    [SerializeField] private float bulletMass = 3;
    [SerializeField] private Transform firePosition;
    [SerializeField] private float fireVelocity;

    [Header("Aim Line Properties")]
    [SerializeField] private LineRenderer aimLine;
    [SerializeField] private int aimLineSegments = 60;
    [SerializeField] private float aimLineTimeOfTheFlight = 5.0f;

    private Camera mainCam;
    private bool isOnAimMode;
    private InputReader inputReader;
    private bool isWaiting = false;

    private void Start() 
    {
        isOnAimMode = false;
        mainCam = Camera.main;
    }

    private void Update() 
    {
        if(!isOnAimMode)
            return;
        
        ShowAimLine(firePosition.position, fireVelocity * mainCam.transform.forward / bulletMass /* mainCam.transform.forward * fireVelocity / bulletMass */);
        if(inputReader.IsPressingLeftMouse && !isWaiting)
        {
            Fire();
        }
    }

    private void Fire()
    {
        GameObject firedBullet = Instantiate(bullet, firePosition.position, Quaternion.identity);
        Rigidbody rb = firedBullet.GetComponent<Rigidbody>();
        rb.mass = bulletMass;
        rb.AddForce(mainCam.transform.forward * fireVelocity, ForceMode.Impulse);
        isWaiting = true;

        StartCoroutine(DelayShooting());
    }

    private IEnumerator DelayShooting()
    {
        yield return new WaitForSeconds(3.0f);
        isWaiting = false;
    }

    #region Aim Line
    /* private void DrawProjection()
    {
        aimLine.enabled = true;
        aimLine.positionCount = Mathf.CeilToInt(aimLineSegments / aimLineTimeOfTheFlight) + 1;
        Vector3 startPosition = firePosition.position;
        Vector3 startVelocity = fireVelocity * mainCam.transform.forward / bulletMass;
        int i = 0;
        aimLine.SetPosition(i, startPosition);
        for (float time = 0; time < aimLineSegments; time += aimLineTimeOfTheFlight)
        {
            i++;
            Vector3 point = startPosition + time * startVelocity;
            point.y = startPosition.y + startVelocity.y * time + (Physics.gravity.y / 2f * time * time);

            aimLine.SetPosition(i, point);

            Vector3 lastPosition = aimLine.GetPosition(i - 1);
        }
    } */

    private void ShowAimLine(Vector3 starPoint, Vector3 startVelocity)
    {
        float timeStep = aimLineTimeOfTheFlight / aimLineSegments;;

        Vector3[] lineRendererPoints = CalculateAimLine(starPoint, startVelocity, timeStep);

        aimLine.positionCount = aimLineSegments;
        aimLine.SetPositions(lineRendererPoints);
    }

    private Vector3[] CalculateAimLine(Vector3 startPoint, Vector3 startVelocity, float timeStep)
    {
        Vector3[] lineRendererPoints = new Vector3[aimLineSegments];

        lineRendererPoints[0] = startPoint;

        for (int i = 1; i < aimLineSegments; i++)
        {
            float timeOffset = timeStep * 1;

            Vector3 progressBeforeGravity = startVelocity * timeOffset;
            Vector3 gravityOffset = Vector3.up * -0.5f * Physics.gravity.y * timeOffset * timeOffset;
            Vector3 newPosition = startPoint + progressBeforeGravity - gravityOffset;

            lineRendererPoints[i] = newPosition;
        }

        return lineRendererPoints;
    }
    #endregion

    #region Interactable events
    public string GetInteractPrompt()
    {
        return "Open";
    }

    public void OnDesinteract(PlayerController playerController) { }

    public void OnInteract(PlayerController playerController)
    {
        // Podemos ter 2 tipos de canhoes os que desparar só para a frente e os que consegues fazer o aim
        // Verificar se tem bullets no inventory do player ou no inventory do cannon
        
        // Load Cannon
        
        // Fire Cannon



        // Get player input reader component
        inputReader = playerController.GetComponent<InputReader>();

        // Set to aim mode
        isOnAimMode = true;
    }
    #endregion
}
