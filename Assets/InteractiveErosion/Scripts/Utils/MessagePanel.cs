﻿//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;
//using System.Text;
//using System;

//public class Message
//{
//    public string caption, message, closeText;
//    public Message(string caption, string message, string closeText)
//    {
//        this.caption = caption; this.message = message; this.closeText = closeText;
//        Game.MessageQueue.Push(this);
//    }
//}
//public class MessagePanel : DragPanel
//{
//    static Vector3 lastDragPosition;
//    public Text caption, message, closeText;   

//    StringBuilder sb = new StringBuilder();
//    // Use this for initialization
//    void Start()
//    {
//        Vector3 position = Vector3.zero;
//        position.Set(lastDragPosition.x - 10f, lastDragPosition.y - 10f, 0);
//        transform.localPosition = position;
//        lastDragPosition = transform.localPosition;
//    }

//    override public void OnDrag(PointerEventData data)
//    {
//        base.OnDrag(data);        
//        lastDragPosition = transform.localPosition;
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        //refresh();
//    }
//    public override void Refresh(Message mess)
//    {
//        caption.text = mess.caption;
//        message.text = mess.message;
//        closeText.text = mess.closeText;
//    }


//    public void show(Message mess)
//    {
        

//        Game.howMuchPausedWindowsOpen++;
//        gameObject.SetActive(true);
//        //this.pa
//        //panelRectTransform = GetComponent<RectTransform>();
//        //canvasRectTransform = GetComponent<RectTransform>();
//        panelRectTransform.SetAsLastSibling();
//        refresh(mess);
//    }
   
//    override public void onCloseClick()
//    {
//        base.onCloseClick();
//        Game.howMuchPausedWindowsOpen--;       
//        Destroy(gameObject);
//    }

//    public override void Refresh()
//    {
//        throw new NotImplementedException();
//    }
//}
