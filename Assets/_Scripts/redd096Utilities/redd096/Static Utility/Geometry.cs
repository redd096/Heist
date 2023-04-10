using UnityEngine;

namespace redd096
{
    public static class Angle
    {
        /// <summary>
        /// Clamp from min to max
        /// </summary>
        public static float ClampAngle(float angle, float min, float max)
        {
            //can't go under -360
            if (angle < -360)
                angle += 360;

            //can't go over 360
            if (angle > 360)
                angle -= 360;

            return Mathf.Clamp(angle, min, max);
        }

        /// <summary>
        /// When we need negative value, like -90 instead of 270, for example with clamp from -90 to 90
        /// </summary>
        public static float NegativeAngle(float angle, float min, float max)
        {
            //if greater than 180, subtract 360 to get negative value
            if (angle > 180)
                angle -= 360;

            return Mathf.Clamp(angle, min, max);
        }
    }

    public static class MathQuaternion
    {
        /// <summary>
        /// originalQuat + addedQuat
        /// </summary>
        public static Quaternion AddQuaternion(Quaternion originalQuat, Quaternion addedQuat)
        {
            return originalQuat * addedQuat;
        }

        /// <summary>
        /// originalQuat - subtractedQuat
        /// </summary>
        public static Quaternion SubtractQuaternion(Quaternion originalQuat, Quaternion subtractedQuat)
        {
            return originalQuat * Quaternion.Inverse(subtractedQuat);
        }
    }

    public static class Direction
    {
        /// <summary>
        /// Return the local direction
        /// </summary>
        public static Vector3 WorldToLocalDirection(Vector3 worldDirection, Quaternion rotation)
        {
            return rotation * worldDirection;
        }

        /// <summary>
        /// Return the local direction using inverse rotation. Used for example to transform rigidbody.velocity to local velocity
        /// </summary>
        public static Vector3 WorldToInverseLocalDirection(Vector3 worldDirection, Quaternion rotation)
        {
            return Quaternion.Inverse(rotation) * worldDirection;
        }

        /// <summary>
        /// Return the world direction
        /// </summary>
        public static Vector3 LocalToWorldDirection(Vector3 localDirection)
        {
            return Quaternion.identity * localDirection;
        }
    }

    public static class TransformRot
    {
        /// <summary>
        /// Transforms rotation from local space to world space. newUp is the up vector of the transform
        /// </summary>
        public static Quaternion TransformRotation(Quaternion rotation, Vector3 newUp)
        {
            return Quaternion.FromToRotation(Vector3.up, newUp) * rotation;
        }

        /// <summary>
        /// Transforms rotation from local space to world space. newUp is the up vector of the transform
        /// </summary>
        public static Quaternion TransformRotation(Vector3 rotation, Vector3 newUp)
        {
            return Quaternion.FromToRotation(Vector3.up, newUp) * Quaternion.Euler(rotation);
        }

        /// <summary>
        /// Transforms rotation from world space to local space. The opposite of TransformRotation.
        /// </summary>
        public static Quaternion InverseTransformRotation(Quaternion rotation)
        {
            //get local up
            Vector3 rotationUp = Direction.WorldToLocalDirection(Vector3.up, rotation);

            return Quaternion.FromToRotation(rotationUp, Vector3.up) * rotation;
        }

        /// <summary>
        /// Transforms rotation from world space to local space. The opposite of TransformRotation.
        /// </summary>
        public static Quaternion InverseTransformRotation(Vector3 rotation)
        {
            //get local up
            Vector3 rotationUp = Direction.WorldToLocalDirection(Vector3.up, Quaternion.Euler(rotation));

            return Quaternion.FromToRotation(rotationUp, Vector3.up) * Quaternion.Euler(rotation);
        }

        /// <summary>
        /// Transforms rotation from local space to new transform local space.
        /// rotation is from the previous transform and newUp is the upVector of the new transform
        /// </summary>
        public static Quaternion TransformToTransformRotation(Quaternion rotation, Vector3 newUp)
        {
            //get local up
            Vector3 rotationUp = Direction.WorldToLocalDirection(Vector3.up, rotation);

            return Quaternion.FromToRotation(rotationUp, newUp) * rotation;
        }

        /// <summary>
        /// Transforms rotation from local space to new transform local space.
        /// rotation is from the previous transform and newUp is the upVector of the new transform
        /// </summary>
        public static Quaternion TransformToTransformRotation(Vector3 rotation, Vector3 newUp)
        {
            //get local up
            Vector3 rotationUp = Direction.WorldToLocalDirection(Vector3.up, Quaternion.Euler(rotation));

            return Quaternion.FromToRotation(rotationUp, newUp) * Quaternion.Euler(rotation);
        }

        /// <summary>
        /// Transforms rotation using local space of another transform, rotated to the local space of another more.
        /// rotation is what we want. prevUp is the upVector of the first transform, newUp is the upVector of the new transform.
        /// Used for example for the camera. We want the camera to use the up vector of the player, also when the camera is rotated
        /// </summary>
        public static Quaternion OtherTransformToTransformRotation(Quaternion rotation, Vector3 prevUp, Vector3 newUp)
        {
            return Quaternion.FromToRotation(prevUp, newUp) * rotation;
        }

        /// <summary>
        /// Transforms rotation using local space of another transform, rotated to the local space of another more.
        /// rotation is what we want. prevUp is the upVector of the first transform, newUp is the upVector of the new transform.
        /// Used for example for the camera. We want the camera to use the up vector of the player, also when the camera is rotated
        /// </summary>
        public static Quaternion OtherTransformToTransformRotation(Vector3 rotation, Vector3 prevUp, Vector3 newUp)
        {
            return Quaternion.FromToRotation(prevUp, newUp) * Quaternion.Euler(rotation);
        }
    }

    public static class RotatedIdentity
    {
        /// <summary>
        /// Rotate the quaternion.identity to a newUp. Then return new quaternion.identity
        /// and difference on y axis (MouseX) from default quaternion.identity and the new identity (for RotationWithNewIdentity)
        /// </summary>
        public static void RotateQuaternionIdentity(Quaternion currentIdentity, Vector3 newUp, out Quaternion newIdentity, out Quaternion differenceYAxisFromDefaultIdentity)
        {
            //to walk on a planet your newUp is player.position - planet.position, or normal using raycast

            //get default quaternion.identity rotated to the new up
            Quaternion defaultIdentityRotated = TransformRot.TransformRotation(Quaternion.identity, newUp);

            //return new Quaternion.identity
            newIdentity = TransformRot.TransformToTransformRotation(currentIdentity, newUp);

            //return difference of rotation on Y axis (MouseX), from Quaternion.identity to new identity
            differenceYAxisFromDefaultIdentity = MathQuaternion.SubtractQuaternion(newIdentity, defaultIdentityRotated);
        }

        /// <summary>
        /// rotation is the rotation we want but with x = right, y = up and z = forward.
        /// We want to rotate it from Vector3.up to our new identity up vector.
        /// We also need difference in y axis from Quaternion.identity to our new identity, because the rotation is applied using z as forward
        /// </summary>
        public static Quaternion RotationWithNewIdentity(Vector3 rotation, Vector3 upVector, Quaternion differenceYAxis)
        {
            //calculate the rotation with new quaternion.identity (so if our Up vector is not Vector3.up, it works anyway)
            Quaternion newRotation = TransformRot.TransformRotation(rotation, upVector);

            //there is a problem: Vector3.up has Z as forward, but rotation can have forward rotated, so we must add Y axis (MouseX)
            return MathQuaternion.AddQuaternion(differenceYAxis, newRotation);
        }

        /// <summary>
        /// NEVER TESTED
        /// Reverse RotationWithNewIdentity, so we get the rotation but rotated with x = right, y = up and z = forward
        /// </summary>
        public static Quaternion InverseRotationWithNewIdentity(Quaternion rotation, Quaternion differenceYAxis)
        {
            //calculate the rotation in world space (so with up vector == Vector3.up)
            Quaternion newRotation = TransformRot.InverseTransformRotation(rotation);

            //there is a problem: Vector3.up has Z as forward, but rotation can have forward rotated, so we must subtract Y axis (MouseX)
            return MathQuaternion.SubtractQuaternion(newRotation, differenceYAxis);
        }
    }
}