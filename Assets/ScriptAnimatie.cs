using UnityEngine;

public class PlayLegacyAnim : MonoBehaviour
{
    void Start()
    {
        Animation anim = GetComponent<Animation>();
        anim.wrapMode = WrapMode.Loop;
        anim.Play();
    }
}
