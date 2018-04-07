using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Блок с указанным списком возможных поворотов
public class RotatingObjectIdentity : PredefinedRotatingObjectIdentity {

    private static int NUM_AXIS = 3;

    public Vector3 allowedRotationMin = Vector3.zero;
    public Vector3 allowedRotationMax = Vector3.zero;

    public bool xyPermutate = false;
    public bool xzPermutate = false;
    public bool yzPermutate = false;
    public bool xyzPermutate = false;

    private List<float>[] allowedRotations = new List<float>[NUM_AXIS];

    protected override void Awake() {

        for(int i = 0; i < NUM_AXIS; i++) {
            var allowedRotation = allowedRotationMin[i];
            allowedRotations[i] = new List<float>() { 0f };
            while (allowedRotation <= allowedRotationMax[i]) {
                if (!allowedRotation.Equals(0f)) {
                    allowedRotations[i].Add(allowedRotation);
                }
                allowedRotation += rotationAngle;
            }
        }

        var additionalRotations = predefinedRotations;
        predefinedRotations = new List<Vector3>();

        for (int x = 0; x < allowedRotations[0].Count; x++) {
            for (int y = 0; y < allowedRotations[1].Count; y++) {
                for (int z = 0; z < allowedRotations[2].Count; z++) {
                    if (!( 
                        (xyPermutate || x == 0 || y == 0) && 
                        (xzPermutate || x == 0 || z == 0) && 
                        (yzPermutate || y == 0 || z == 0 ) && 
                        (xyzPermutate || x == 0 || y == 0 || z == 0) 
                    )) continue;
                    var xx = allowedRotations[0][x];
                    var yy = allowedRotations[1][y];
                    var zz = allowedRotations[2][z];
                    predefinedRotations.Add(new Vector3(xx, yy, zz));
                }
            }
        }

        if(predefinedRotationIndex != 0) {
            predefinedRotationIndex += predefinedRotations.Count;
        }

        predefinedRotations.AddRange(additionalRotations);

        UpdateRotationIndex();
    }

}
