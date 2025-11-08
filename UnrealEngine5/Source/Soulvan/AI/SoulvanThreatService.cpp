// Copyright Epic Games, Inc. All Rights Reserved.
// Soulvan Threat Service Implementation

#include "SoulvanThreatService.h"
#include "BehaviorTree/BlackboardComponent.h"
#include "AIController.h"
#include "GameFramework/Pawn.h"
#include "WheeledVehiclePawn.h"

UBTService_ThreatUpdate::UBTService_ThreatUpdate()
{
	NodeName = "Soulvan Threat Update";
	Interval = 0.5f; // Update every 0.5s
	RandomDeviation = 0.1f;
}

void UBTService_ThreatUpdate::TickNode(UBehaviorTreeComponent& OwnerComp, uint8* NodeMemory, float DeltaSeconds)
{
	Super::TickNode(OwnerComp, NodeMemory, DeltaSeconds);

	UBlackboardComponent* Blackboard = OwnerComp.GetBlackboardComponent();
	if (!Blackboard) return;

	AAIController* AIController = OwnerComp.GetAIOwner();
	if (!AIController) return;

	APawn* SelfPawn = AIController->GetPawn();
	if (!SelfPawn) return;

	// Get blackboard values
	AActor* Rival = Cast<AActor>(Blackboard->GetValueAsObject(RivalKey.SelectedKeyName));
	FVector LastThreatPos = Blackboard->GetValueAsVector(LastThreatPosKey.SelectedKeyName);
	float DamagePct = Blackboard->GetValueAsFloat(DamagePctKey.SelectedKeyName);

	// Get vehicle speed (assuming WheeledVehiclePawn or custom movement)
	float SpeedKmh = 0.f;
	if (AWheeledVehiclePawn* Vehicle = Cast<AWheeledVehiclePawn>(SelfPawn))
	{
		SpeedKmh = Vehicle->GetVehicleMovementComponent()->GetForwardSpeed() * 0.036f; // cm/s to km/h
	}
	else
	{
		SpeedKmh = SelfPawn->GetVelocity().Size() * 0.036f;
	}

	// Calculate threat
	float Threat = CalculateThreat(Rival, LastThreatPos, SpeedKmh, DamagePct, SelfPawn->GetActorLocation());

	// Update blackboard
	Blackboard->SetValueAsFloat(ThreatLevelKey.SelectedKeyName, Threat);
	Blackboard->SetValueAsFloat(SpeedKmhKey.SelectedKeyName, SpeedKmh);
	
	// Calculate motif intensity (performance-scaled in game)
	float MotifIntensity = FMath::Clamp(0.4f + Threat * 0.6f, 0.f, 1.f);
	Blackboard->SetValueAsFloat(MotifIntensityKey.SelectedKeyName, MotifIntensity);
}

float UBTService_ThreatUpdate::CalculateThreat(AActor* Rival, FVector PolicePos, float Speed, float Damage, FVector SelfPos)
{
	// Inverse proximity scoring
	float RivalProx = 0.f;
	if (Rival)
	{
		float Dist = FVector::Distance(SelfPos, Rival->GetActorLocation());
		RivalProx = 1.f / FMath::Max(1.f, Dist);
	}

	float PoliceProx = 0.f;
	if (!PolicePos.IsZero())
	{
		float Dist = FVector::Distance(SelfPos, PolicePos);
		PoliceProx = 1.f / FMath::Max(1.f, Dist);
	}

	// Normalize speed and damage
	float SpeedRisk = FMath::Clamp(Speed / MaxSpeedKmh, 0.f, 1.f);
	float DamageRisk = FMath::Clamp(Damage, 0.f, 1.f);

	// Weighted sum
	float Threat = RivalWeight * RivalProx +
	               PoliceWeight * PoliceProx +
	               SpeedWeight * SpeedRisk +
	               DamageWeight * DamageRisk;

	return FMath::Clamp(Threat, 0.f, 1.f);
}
