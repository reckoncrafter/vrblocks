/*
 Credit: https://github.com/vazgriz/PID_Controller/blob/master/Assets/Scripts/PID_Controller.cs
*/
using System;
using System.Collections.Generic;
using UnityEngine;

public class PIDController {
    public enum DerivativeMeasurement {
        Velocity,
        ErrorRateOfChange
    }

    //PID coefficients
    public float proportionalGain = 0.01f;
    public float integralGain = 0.001f;
    public float derivativeGain = 0.5f;

    public float outputMin = -1;
    public float outputMax = 1;
    public float integralSaturation = 1.0f;
    public DerivativeMeasurement derivativeMeasurement;

    public float valueLast;
    public float errorLast;
    public float integrationStored;
    public float velocity;  //only used for the info display
    public bool derivativeInitialized;

    public void Reset() {
        derivativeInitialized = false;
    }

    public float Update(float dt, float currentValue, float targetValue) {
        if (dt <= 0) throw new ArgumentOutOfRangeException(nameof(dt));

        float error = targetValue - currentValue;

        //calculate P term
        float P = proportionalGain * error;

        //calculate I term
        integrationStored = Mathf.Clamp(integrationStored + (error * dt), -integralSaturation, integralSaturation);
        float I = integralGain * integrationStored;

        //calculate both D terms
        float errorRateOfChange = (error - errorLast) / dt;
        errorLast = error;

        float valueRateOfChange = (currentValue - valueLast) / dt;
        valueLast = currentValue;
        velocity = valueRateOfChange;

        //choose D term to use
        float deriveMeasure = 0;

        if (derivativeInitialized) {
            if (derivativeMeasurement == DerivativeMeasurement.Velocity) {
                deriveMeasure = -valueRateOfChange;
            } else {
                deriveMeasure = errorRateOfChange;
            }
        } else {
            derivativeInitialized = true;
        }

        float D = derivativeGain * deriveMeasure;

        float result = P + I + D;

        return Mathf.Clamp(result, outputMin, outputMax);
    }
}