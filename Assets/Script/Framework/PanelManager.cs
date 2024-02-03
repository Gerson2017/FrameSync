using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelManager : Singleton<PanelManager>
{
    /// <summary>
    /// 层级
    /// </summary>
    public enum Layer
    {
        Panel,
        Tip
    }
    /// <summary>
    /// 层级列表
    /// </summary>
    private Dictionary<Layer,Transform> layers = new Dictionary<Layer,Transform>();
    /// <summary>
    /// 面板列表
    /// </summary>
    private Dictionary<string,BasePanel> panels = new Dictionary<string,BasePanel>();
    /// <summary>
    /// 面板根级元素
    /// </summary>
    private Transform root;
    /// <summary>
    /// 画布
    /// </summary>
    private Transform canvas;
    protected override void Awake()
    {
        base.Awake();
        Init();
    }
    /// <summary>
    /// 初始化
    /// </summary>
    public void Init()
    {
        root = GameObject.Find("Root").transform;
        canvas = root.Find("Canvas");
        layers.Add(Layer.Panel, canvas.Find("Panel"));
        layers.Add(Layer.Tip, canvas.Find("Tip"));
    }
    /// <summary>
    /// 打开面板
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="args"></param>
    public void Open<T>(params object[] args) where T : BasePanel
    {
        string name=typeof(T).Name;
        //已经打开
        if(panels.ContainsKey(name)) 
        {
            return;
        }
        BasePanel panel = root.gameObject.AddComponent<T>();
        panel.OnInit();
        panel.Init();

        Transform layer = layers[panel.layer];
        panel.skin.transform.SetParent(layer,false);
        panels.Add(name, panel);
        panel.OnShow();
    }
    /// <summary>
    /// 关闭面板
    /// </summary>
    /// <param name="name">面板名字</param>
    public void Close(string name)
    {
        //判断是否有这个面板
        if(!panels.ContainsKey(name))
        {
            return;
        }
        BasePanel panel = panels[name];
        panel.OnClose();
        panels.Remove(name);
        Destroy(panel.skin);
        Destroy(panel);
    }
}
