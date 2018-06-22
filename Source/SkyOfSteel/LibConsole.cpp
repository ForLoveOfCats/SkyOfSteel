#include "LibConsole.h"



bool DidPrepareCommandList = false;


void ULibConsole::RunCommand()
{
	GEngine->AddOnScreenDebugMessage(-1, 5.f, FColor::Blue, FString(DidPrepareCommandList ? TEXT("true") : TEXT("false")));
	DidPrepareCommandList = true;
}

