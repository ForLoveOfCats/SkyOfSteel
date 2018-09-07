extends Node

const SimilarAmount = 0.5

func vec_similar(vec1, vec2):
	#if abs(abs(vec1.x)-abs(vec2.x)) > SimilarAmount or abs(abs(vec1.y)-abs(vec2.y)) > SimilarAmount or abs(abs(vec1.z)-abs(vec2.z)) > SimilarAmount:
	if vec1.distance_squared_to(vec2) > SimilarAmount*SimilarAmount:
		return false
	return true
