using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public string sceneName;// ���[�h����V�[���̖��O
    public GameObject loadingUI;// ���[�h�̐i���󋵂�\������UI�Ȃ�
    public Button[] loadButtons;// �����̃{�^�����������߂̔z��
    private AsyncOperation async;// ���[�h�̐i���󋵂��Ǘ����邽�߂̕ϐ�

    //// ���[�h�ɂ�����Œ᎞�ԁi�b�j
    //[SerializeField] protected float minimumLoadTime = 1f;
    //// �{�^���̖�������
    //[SerializeField] protected float buttonCooldownTime = 5f;

    // ���[�h���J�n���郁�\�b�h
    public void StartLoad()
    {
        StartCoroutine(Load());
        DisableButtons();
    }

    // �R���[�`�����g�p���ă��[�h�����s���郁�\�b�h
    private IEnumerator Load()
    {
        PlayerPrefs.Save();
        loadingUI.SetActive(true);// ���[�h��ʂ�\������
        async = SceneManager.LoadSceneAsync(sceneName);// �V�[����񓯊��Ń��[�h����
        async.allowSceneActivation = false;// ���[�h���������Ă������ɃV�[����L���ɂ��Ȃ��i�i�s�󋵂�0.9�Ŏ~�܂�j
        float elapsedTime = 0f;// ���[�h�J�n����̌o�ߎ��Ԃ��v��
        while (!async.isDone)// ���[�h���������邩�A�ŒჍ�[�h���Ԃ��o�߂���܂őҋ@����
        {
            elapsedTime += UnityEngine.Time.deltaTime;
            if (async.progress >= 0.9f && elapsedTime >= 1f)
            {
                async.allowSceneActivation = true;
            }
            yield return null;
        }
    // ���[�h��ʂ��\���ɂ���
    loadingUI.SetActive(false);
    }
    private void DisableButtons()
    {
        foreach (Button button in loadButtons)
        {
            if (button != null)
            {
                button.interactable = false;
            }
        }
    }
}