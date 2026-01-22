using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class RadialProgress : VisualElement
{
    // Cores padrão BEM VISÍVEIS para garantir que apareça
    [UxmlAttribute]
    public Color progressColor = new Color(1f, 0.5f, 0f, 1f); // Laranja Opaco
    
    [UxmlAttribute]
    public Color trackColor = new Color(0.2f, 0.2f, 0.2f, 1f); // Cinza Escuro Opaco

    [UxmlAttribute]
    public float thickness = 10f;

    private float m_Progress = 0f;
    [UxmlAttribute]
    public float Progress
    {
        get => m_Progress;
        set
        {
            m_Progress = Mathf.Clamp01(value);
            MarkDirtyRepaint();
        }
    }

    public RadialProgress()
    {
        generateVisualContent += OnGenerateVisualContent;
        
        // Define um tamanho padrão para não sumir se o layout falhar
        
        
        // Garante que o fundo do container seja transparente para não cobrir o desenho
        style.backgroundColor = Color.clear; 
    }

    void OnGenerateVisualContent(MeshGenerationContext ctx)
    {
        var painter = ctx.painter2D;
        float width = contentRect.width;
        float height = contentRect.height;

        // Evita desenhar se não tiver tamanho
        if (width < 1 || height < 1) return;

        float radius = Mathf.Min(width, height) / 2f;
        Vector2 center = new Vector2(width / 2f, height / 2f);

        painter.lineWidth = thickness;
        painter.lineCap = LineCap.Round; // Borda arredondada fica mais bonito!

        // 1. TRACK (Cinza)
        if (trackColor.a > 0.01f)
        {
            painter.strokeColor = trackColor;
            painter.BeginPath();
            painter.Arc(center, radius - (thickness * 0.5f), 0f, 360f);
            painter.Stroke();
        }

        // 2. PROGRESS (Laranja)
        if (m_Progress > 0.01f && progressColor.a > 0.01f)
        {
            painter.strokeColor = progressColor;
            painter.BeginPath();
            
            // Ângulo -90 (topo)
            float startAngle = -90f;
            float endAngle = startAngle + (m_Progress * 360f);

            painter.Arc(center, radius - (thickness * 0.5f), startAngle, endAngle);
            painter.Stroke();
        }
    }
}