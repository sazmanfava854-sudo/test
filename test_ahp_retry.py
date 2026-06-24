# -*- coding: utf-8 -*-
"""Automated test for AHP pairwise retry when CR > 0.10."""
from unittest.mock import patch
from iot_selector import IoTSelector

# 21 pairwise comparisons per attempt (7 criteria, upper triangle)
# Attempt 1 — deliberate inconsistency: Cost>Energy, Energy>Link, Link>Cost
INCONSISTENT = [
  "7", "1/7", "1", "1", "1", "1",   # هزینه vs ...
  "7", "1", "1", "1", "1",          # مصرف انرژی vs ...
  "1", "1", "1", "1",               # بودجه لینک vs ...
  "1", "1", "1",                    # تاخیر vs ...
  "1", "1",                         # سلولی vs ...
  "1",                              # میزان داده vs برد
]

# Attempt 2 — consistent values (per test plan)
CONSISTENT = [
  "3", "5", "1", "1", "1", "1",
  "2", "1", "1", "1", "1",
  "1", "1", "1", "1",
  "1", "1", "1",
  "1", "1",
  "1",
]

INPUTS = INCONSISTENT + ["r"] + CONSISTENT


def main():
  selector = IoTSelector(include_cellular_in_clustering=False)
  user_answers = {}  # no questionnaire rules — isolate pairwise AHP

  print("=" * 60)
  print("TEST: AHP retry on high CR")
  print("=" * 60)

  with patch("builtins.input", side_effect=INPUTS):
    result = selector.get_user_preferences_pairwise(user_answers)

  print("\n" + "=" * 60)
  print("TEST RESULT")
  print("=" * 60)
  print(f"CR = {result['cr']:.4f}")
  print(f"Status: {result['cr_status']}")
  print(f"Input mode: {result.get('input_mode')}")
  print("\nFinal weights:")
  for label, w in result['weights'].items():
    print(f"  {label:<20} {w:.4f}")

  assert result['cr'] <= 0.10, f"Expected CR <= 0.10, got {result['cr']}"
  assert result.get('input_mode') == 'user_pairwise'
  print("\n✅ TEST PASSED: retry worked, CR acceptable on second attempt.")


if __name__ == "__main__":
  main()
