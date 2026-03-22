import numpy as np
import pandas as pd


class StatisticalRiskTools:
    @staticmethod
    def claim_frequency_risk(claims: int) -> float:
        return float(min(claims / 10.0, 1.0))

    @staticmethod
    def claim_amount_risk(amount: float) -> float:
        # Log scale keeps high amounts impactful but bounded.
        return float(min(np.log1p(amount) / 12.0, 1.0))

    @staticmethod
    def age_risk(age: int) -> float:
        # Mid-age tends to be lower-risk than very young/very old.
        center = 45
        normalized = abs(age - center) / 55.0
        return float(min(normalized, 1.0))

    @staticmethod
    def combined_score(age: int, claims: int, amount: float) -> float:
        frame = pd.DataFrame([
            {
                "age_risk": StatisticalRiskTools.age_risk(age),
                "freq_risk": StatisticalRiskTools.claim_frequency_risk(claims),
                "amount_risk": StatisticalRiskTools.claim_amount_risk(amount),
            }
        ])

        weighted = (
            frame["age_risk"] * 0.20
            + frame["freq_risk"] * 0.35
            + frame["amount_risk"] * 0.45
        )

        return float(np.clip(weighted.iloc[0], 0.0, 1.0))
