using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CharacterMessageManager : MonoBehaviour
{
    public Canvas CharCanvas;
    public RectTransform CharRectTransform;
    //private Queue<string> _messageBuffer = new Queue<string>(3);

    public void AddMessage(string message)
    {
        //_messageBuffer.Enqueue(message);
        //RefreshDisplay();

        var obj = (RectTransform)Instantiate(CharRectTransform, Vector3.zero, Quaternion.identity);
        obj.parent = CharCanvas.transform;
        obj.localPosition = new Vector3(20, 20, 0);
        obj.localRotation = Quaternion.identity;
        obj.localScale = Vector3.one;
        obj.GetComponent<Text>().text = message;
        obj.GetComponent<Animation>().Play();

        StartCoroutine(DisplayAndDestroyMessage(obj));

    }

    IEnumerator DisplayAndDestroyMessage(Transform obj)
    {
        yield return new WaitForSeconds(1f);
        Destroy(obj.gameObject);
    }

    private void RefreshDisplay()
    {
        //Message.text = string.Empty;
        //foreach (var item in _messageBuffer.ToList())
        //{
        //    Message.text += (item + "\n");
        //}
    }

}
