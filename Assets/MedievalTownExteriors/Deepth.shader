Shader "Custom/shader2"
{
  subshader {
        Tags { "RenderType"="Opaque" }
    pass {

    CGPROGRAM
    #pragma vertex vert
    #pragma fragment frag
    #include "UnityCG.cginc"

      sampler2D _CameraDepthTexture; // la texture contenant les valeurs de profondeurs de la caméra principale (affectation automatique par Unity)

        struct outV {
          float4 pos : SV_POSITION;
          float4 fTex : TEXCOORD0;
        };

        struct inV {
          float4 position : POSITION;
        };



        outV vert(inV v) {
          outV res;

          res.pos = UnityObjectToClipPos(v.position);
          res.fTex=res.pos;
          return res;
        }

        float4 frag(outV v) : COLOR {
          float2 gett; // calculer screen coordinate dans [0,1]
          gett.x=(v.fTex.x/v.fTex.w)*0.5+0.5;
          gett.y=0.5-(v.fTex.y/v.fTex.w)*0.5;
          float f=tex2D(_CameraDepthTexture,gett.xy); // lire la valeur de depth
          return float4(f,0,0,1); // valeur de depth en couleur rouge
      }
    ENDCG
    }
  }
}