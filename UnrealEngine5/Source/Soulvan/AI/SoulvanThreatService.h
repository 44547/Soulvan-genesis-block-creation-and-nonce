// Copyright Epic Games, Inc. All Rights Reserved.
// Soulvan AI Threat Evaluation Service for Behavior Trees

#pragma once

#include "CoreMinimal.h"
#include "BehaviorTree/BTService.h"
#include "SoulvanThreatService.generated.h"

/**
 * Behavior Tree Service that evaluates threat level based on rival proximity,
 * police distance, speed risk, and vehicle damage.
 * Updates Blackboard keys: ThreatLevel, SpeedKmh, MotifIntensity
 */
UCLASS()
class SOULVAN_API UBTService_ThreatUpdate : public UBTService
{
	GENERATED_BODY()

public:
	UBTService_ThreatUpdate();

protected:
	virtual void TickNode(UBehaviorTreeComponent& OwnerComp, uint8* NodeMemory, float DeltaSeconds) override;

	// Blackboard keys
	UPROPERTY(EditAnywhere, Category = "Blackboard")
	FBlackboardKeySelector RivalKey;

	UPROPERTY(EditAnywhere, Category = "Blackboard")
	FBlackboardKeySelector LastThreatPosKey;

	UPROPERTY(EditAnywhere, Category = "Blackboard")
	FBlackboardKeySelector ThreatLevelKey;

	UPROPERTY(EditAnywhere, Category = "Blackboard")
	FBlackboardKeySelector SpeedKmhKey;

	UPROPERTY(EditAnywhere, Category = "Blackboard")
	FBlackboardKeySelector DamagePctKey;

	UPROPERTY(EditAnywhere, Category = "Blackboard")
	FBlackboardKeySelector MotifIntensityKey;

	// Weights for threat calculation
	UPROPERTY(EditAnywhere, Category = "Threat")
	float RivalWeight = 0.45f;

	UPROPERTY(EditAnywhere, Category = "Threat")
	float PoliceWeight = 0.35f;

	UPROPERTY(EditAnywhere, Category = "Threat")
	float SpeedWeight = 0.15f;

	UPROPERTY(EditAnywhere, Category = "Threat")
	float DamageWeight = 0.05f;

	UPROPERTY(EditAnywhere, Category = "Threat")
	float MaxSpeedKmh = 220.f;

private:
	float CalculateThreat(AActor* Rival, FVector PolicePos, float Speed, float Damage, FVector SelfPos);
};
