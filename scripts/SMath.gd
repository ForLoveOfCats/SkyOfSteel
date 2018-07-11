extends Node

const SimilarAmmount = 3

func vec_similar(vec1, vec2):
	if abs(vec1.x-vec2.x) > SimilarAmmount or abs(vec1.y-vec2.y) > SimilarAmmount or abs(vec1.z-vec2.z) > SimilarAmmount:
		return false
	return true
