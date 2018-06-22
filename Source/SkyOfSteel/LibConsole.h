#pragma once

#include "CoreMinimal.h"
#include "Kismet/BlueprintFunctionLibrary.h"
#include "LibConsole.generated.h"



UCLASS()

class SKYOFSTEEL_API ULibConsole : public UBlueprintFunctionLibrary
{
	GENERATED_BODY()

	public:
		UFUNCTION(BlueprintCallable)
		static void RunCommand();
};
