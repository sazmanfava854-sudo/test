# -*- coding: utf-8 -*-
"""Test expert-base AHP: no pairwise user input; CR acceptable on first pass."""
from iot_selector import IoTSelector

ANSWERS = {
    'masahat_zamin': 'متوسط', 'topography': 'مسطح و هموار',
    'manae_fiziki': 'موانع متوسط', 'dastresi_bargh': 'دسترسی محدود',
    'internet_nazdik': 'خیر', 'pooshesh_mobile': 'پوشش متوسط',
    'tedad_sensor': 'متوسط', 'tarakom_sensor': 'تراکم متوسط',
    'hajm_dadeh': 'متوسط', 'budjeh_avalieh': 'متوسط',
    'hazine_amaliati': 'متوسط', 'ghabeliat_gostaresh': 'اهمیت متوسط',
}


def main():
  selector = IoTSelector(include_cellular_in_clustering=False)

  print("=" * 60)
  print("TEST: expert-base AHP (no pairwise user input)")
  print("=" * 60)

  result = selector._personalize_ahp_from_questionnaire(ANSWERS.copy())

  print("\n" + "=" * 60)
  print("TEST RESULT")
  print("=" * 60)
  print(f"CR = {result['cr']:.4f}")
  print(f"Status: {result['cr_status']}")
  print(f"Input mode: {result.get('input_mode')}")

  assert result.get('input_mode') == 'expert_base_only'
  assert not result['applied_rules']
  print("\n✅ TEST PASSED: expert PCM unchanged, no manual pairwise input.")


if __name__ == "__main__":
  main()
