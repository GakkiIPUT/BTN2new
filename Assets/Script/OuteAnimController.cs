using UnityEngine;

public class OuteAnimController : MonoBehaviour
{
    //===== ��`�̈� =====
    public static Animator anim;  //Animator��anim�Ƃ����ϐ��Œ�`����

    //===== �������� =====
    void Start()
    {
        //�ϐ�anim�ɁAAnimator�R���|�[�l���g��ݒ肷��
        anim = gameObject.GetComponent<Animator>();
    }

    //===== �又�� =====
    public static void PlayOuteAnim()
    {
        Debug.Log("�Ă΂ꂽ");
        //Bool�^�̃p�����[�^�[�ł���blRot��True�ɂ���
        anim.SetBool("Blpos", true);
        Debug.Log("Blpos�ς���");
    }
}
