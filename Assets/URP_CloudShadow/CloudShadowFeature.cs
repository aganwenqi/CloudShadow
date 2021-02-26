using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CloudShadowFeature : ScriptableRendererFeature
{
    public RenderPassEvent renderPassEvent;

    [Tooltip("������ɫ")]
    public Color m_Color = Color.white;
    [Tooltip("XYΪ��ʼλ�ã�zwΪXY�ƶ��ٶ�")]
    public Vector4 m_StartXYSpeedXY = new Vector4(5, 5, 0, 0);
    [Tooltip("��ͼ���Ŵ�С��ֵԽСԽ��")]
    public float m_Scale = 0.1f;
    [Tooltip("�����ƶ��Ĵ�С��Χ")]
    public float m_WorldSize = 200;
    public Texture2D m_Tex;
    CloudShadowPass m_Pass;
    //feature������ʱ����
    public override void Create()
    {
        var mat = new Material(Shader.Find("Hidden/CloudShadow"));
        mat.SetColor("_Color", m_Color);
        mat.SetTexture("_CloudTex", m_Tex);
        mat.SetVector("_StartXYSpeedXY", m_StartXYSpeedXY);
        mat.SetFloat("_Scale", m_Scale);
        mat.SetFloat("_WorldSize", m_WorldSize);
        m_Pass = new CloudShadowPass(mat, renderPassEvent);
    }
    //ÿһ֡���ᱻ����
    public override void AddRenderPasses(UnityEngine.Rendering.Universal.ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        //����ǰ��Ⱦ����ɫRT����Pass��
        m_Pass.Setup(renderer.cameraColorTarget);
        //�����pass��ӵ���Ⱦ����
        renderer.EnqueuePass(m_Pass);
    }
}
