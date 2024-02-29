void CalculateCorners_float(
	float2 UV, float2 TexelSize, float Distance,
	out float2 Center, out float2 TopRight, out float2 BottomLeft, out float2 TopLeft, out float2 BottomRight, 
	out float2 Right, out float2 Left, out float2 Top, out float2 Bottom)
{
	// cos(45') = 0.707...
	Center = UV;
	TopRight = UV + float2(+1, +1) * Distance * 0.707f * TexelSize;
	BottomLeft = UV + float2(-1, -1) * Distance * 0.707f * TexelSize;
	TopLeft = UV + float2(-1, +1) * Distance * 0.707f * TexelSize;
	BottomRight = UV + float2(+1, -1) * Distance * 0.707f * TexelSize;
	Right = UV + float2(+1, 0) * Distance * TexelSize;
	Left = UV + float2(-1, 0) * Distance * TexelSize;
	Top = UV + float2(0, +1) * Distance * TexelSize;
	Bottom = UV + float2(0, -1) * Distance * TexelSize;
}