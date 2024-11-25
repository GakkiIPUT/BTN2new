using UnityEngine;

public class OuteAnimController : MonoBehaviour
{
    //===== 定義領域 =====
    public static Animator anim;  //Animatorをanimという変数で定義する

    //===== 初期処理 =====
    void Start()
    {
        //変数animに、Animatorコンポーネントを設定する
        anim = gameObject.GetComponent<Animator>();
    }

    //===== 主処理 =====
    public static void PlayOuteAnim()
    {
        Debug.Log("呼ばれた");
        //Bool型のパラメーターであるblRotをTrueにする
        anim.SetBool("Blpos", true);
        Debug.Log("Blpos変えた");
    }
}
