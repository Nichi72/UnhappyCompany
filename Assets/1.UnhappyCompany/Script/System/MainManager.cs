using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainManager : MonoBehaviour
{
    //public List<string> scenes;
    public string coreScene;
    public string mapScene;



    private void Start()
    {

    }

    public void BtnEvtStartScene()
    {
        StartCoroutine(LoadYourAsyncScene(coreScene, LoadSceneMode.Single));
        StartCoroutine(LoadYourAsyncScene(mapScene, LoadSceneMode.Additive));
    }

    IEnumerator LoadYourAsyncScene(string sceneName , LoadSceneMode loadSceneMode)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);

        while (!asyncLoad.isDone)
        {
            // 로딩 진행 상황 (0~1 사이의 값)
            Debug.Log(asyncLoad.progress);
            yield return null;
        }
    }
}
