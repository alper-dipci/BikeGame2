using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DefaultNamespace;

public class CircleWipeController : MonoBehaviour
{
    [SerializeField] private Image Image; // inspector'a sürükle
    [SerializeField] private Material wipeMat; // shader'dan oluşturduğun materyal
    [SerializeField] private float duration = 0.8f;
    [SerializeField] private float transitionTime = 0.2f; // geçiş süresi

    Coroutine _wipeCoroutine;
    private float _currentRadius;

    void Start()
    {
        if (wipeMat == null && Image != null) wipeMat = Image.material;
        GameManager.Instance.OnPlayerFailed += CloseAndOpenWipe;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            OpenWipe();
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            CloseWipe();
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            CloseAndOpenWipe();
        }
    }
    public void CloseAndOpenWipe()
    {
        if (_wipeCoroutine != null)
        {
            StopCoroutine(_wipeCoroutine);
            _wipeCoroutine = null;
        }
        _wipeCoroutine = StartCoroutine(OpenAndCloseWipe());
    }

    private IEnumerator OpenAndCloseWipe()
    {
        Image.enabled = true; // wipe işlemi başlamadan önce resmi göster
        yield return StartCoroutine(WipeCoroutine(_currentRadius, 1f, duration));
        yield return new WaitForSeconds(transitionTime);
        yield return StartCoroutine(WipeCoroutine(_currentRadius, -1.5f, duration));
        Image.enabled = false; // wipe işlemi tamamlandığında resmi gizle
    }
    

    // dışarıdan çağıracağın fonksiyon: ekranı karart (dıştan içe)
    public void OpenWipe()
    {
        if (_wipeCoroutine != null)
        {
            StopCoroutine(_wipeCoroutine);
            _wipeCoroutine = null;
        }
        Image.enabled = true; // wipe işlemi başlamadan önce resmi göster
        _wipeCoroutine = StartCoroutine(WipeCoroutine(_currentRadius, 1f, duration));
    }

    public void CloseWipe()
    {
        if (_wipeCoroutine != null)
        {
            StopCoroutine(_wipeCoroutine);
            _wipeCoroutine = null;
        }

        _wipeCoroutine = StartCoroutine(WipeCoroutine(_currentRadius, -1.5f, duration));
        Image.enabled = false; // wipe işlemi tamamlandığında resmi gizle
    }

    IEnumerator WipeCoroutine(float from, float to, float time)
    {
        float t = 0f;
        while (t < time)
        {
            float v = Mathf.Lerp(from, to, t / time);
            wipeMat.SetFloat("_Radius", v);
            _currentRadius = v;
            t += Time.deltaTime;
            yield return null;
        }

        wipeMat.SetFloat("_Radius", to);
        _currentRadius = to;
    }
}