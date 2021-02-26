using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CloudShadowPass : ScriptableRenderPass
{
    Material m_Material;

    RenderTargetIdentifier m_Source;
    //RT��Filter
    public FilterMode filterMode { get; set; }
    //����RT
    RenderTargetHandle m_TemporaryColorTexture;

    //Profiling����ʾ
    ProfilingSampler m_ProfilingSampler = new ProfilingSampler("CloudShadow");
    public CloudShadowPass(Material mat, RenderPassEvent passEvenet)
    {
        m_Material = mat;
        this.renderPassEvent = passEvenet;
        //��ʼ������RT����
        m_TemporaryColorTexture.Init("CloudShadow");
    }
    public void Setup(RenderTargetIdentifier source)
    {
        m_Source = source;
    }
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var currentCamera = renderingData.cameraData.camera;
        var aspect = currentCamera.aspect;
        var far = currentCamera.farClipPlane;
        var right = currentCamera.transform.right;
        var up = currentCamera.transform.up;
        var forward = currentCamera.transform.forward;
        var halfFovTan = Mathf.Tan(currentCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);

        //���������Զ�ü��洦��xyz����������
        var rightVec = right * far * halfFovTan * aspect;
        var upVec = up * far * halfFovTan;
        var forwardVec = forward * far;

        //�����ĸ��ǵķ�������
        var topLeft = (forwardVec - rightVec + upVec);
        var topRight = (forwardVec + rightVec + upVec);
        var bottomLeft = (forwardVec - rightVec - upVec);
        var bottomRight = (forwardVec + rightVec - upVec);

        var viewPortRay = Matrix4x4.identity;

        viewPortRay.SetRow(0, bottomLeft);
        viewPortRay.SetRow(1, topLeft);
        viewPortRay.SetRow(2, topRight);
        viewPortRay.SetRow(3, bottomRight);
        //viewPortRay = viewPortRay * m_RotateXMatri;
        m_Material.SetMatrix("_ViewPortRay", viewPortRay);
        
        CommandBuffer cmd = CommandBufferPool.Get();
        //using���������ǿ�����FrameDebug�Ͽ��������������Ⱦ
        
        using (new ProfilingScope(cmd, m_ProfilingSampler))
        {
            //����һ��RT
            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDesc.depthBufferBits = 0;
            cmd.GetTemporaryRT(m_TemporaryColorTexture.id, opaqueDesc, filterMode);

            Blit(cmd, m_Source, m_TemporaryColorTexture.Identifier(), m_Material);
            Blit(cmd, m_TemporaryColorTexture.Identifier(), m_Source);
        }
        //ִ��
        context.ExecuteCommandBuffer(cmd);

        //����
        CommandBufferPool.Release(cmd);
    }
    public override void FrameCleanup(CommandBuffer cmd)
    {
        base.FrameCleanup(cmd);
        //���ٴ�����RT
        cmd.ReleaseTemporaryRT(m_TemporaryColorTexture.id);
    }
}
