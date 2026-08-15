void CalculateIntersectionPoint_float(float3 planePoint, float3 lookPosition, float3 startPoint, float3 direction, out float2 uv)
{
	float3 planeNormal = normalize(lookPosition - planePoint);
	float distanceToPlane = dot(planePoint - startPoint, planeNormal) / dot(direction, planeNormal);
	float3 intersectionPoint = startPoint + distanceToPlane * direction;


	intersectionPoint -= planePoint;


	float3 planeU = normalize(cross(float3(0, 1, 0), planeNormal));


	if(length(planeU) < 0.0001)
	{
		planeU = float3(1, 0, 0);
	}

	float3 planeV = cross(planeNormal, planeU);

	uv.x = dot(intersectionPoint, planeU);
	uv.y = dot(intersectionPoint, planeV);
}