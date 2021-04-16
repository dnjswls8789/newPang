Shader "Seraph/Particle_EqualOneAdd"
{
	Properties
	{

	_MainTex("Particle Texture",2D) = "white"{}
	}

		Category{
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
		Blend SrcAlpha One
		Cull Off
		Lighting Off
		ZWrite Off
		Fog { Color(0,0,0,0)}

		Stencil
		{
		Ref 1
		//Comp notEqual
		Comp Equal
		Pass keep
		}



		BindChannels {
		Bind "Color", color
		Bind "Vertex", vertex
		Bind "TexCoord", texcoord
		}

		SubShader {
		pass {
		SetTexture[_MainTex]{
		combine texture * primary
		}
		}
		}

	}

}