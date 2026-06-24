# -*- coding: utf-8 -*-
"""Restrictive-but-valid answers must not be flagged as contradictions."""
from iot_selector import IoTSelector

RESTRICTIVE_ANSWERS = {
    'internet_nazdik': 'خیر',
    'hajm_dadeh': 'کم',
    'budjeh_avalieh': 'محدود',
    'hazine_amaliati': 'بسیار محدود',
    'masahat_zamin': 'متوسط',
    'topography': 'مسطح و هموار',
    'manae_fiziki': 'موانع متوسط',
    'dastresi_bargh': 'دسترسی محدود',
    'pooshesh_mobile': 'پوشش متوسط',
    'tedad_sensor': 'متوسط',
    'tarakom_sensor': 'تراکم متوسط',
    'ghabeliat_gostaresh': 'اهمیت متوسط',
}


def main():
  selector = IoTSelector()
  logical = selector._find_logical_contradictions(RESTRICTIVE_ANSWERS)
  assert not logical, f"Restrictive profile must not trigger logical conflicts: {logical}"
  assert selector._is_restrictive_valid_profile(RESTRICTIVE_ANSWERS)

  result = selector._personalize_ahp_from_questionnaire(RESTRICTIVE_ANSWERS.copy())
  assert result['input_mode'] in (
    'expert_base_plus_rules',
    'expert_base_plus_rules_constrained',
    'expert_base_only',
  ), f"Unexpected mode: {result.get('input_mode')}"
  print(f"mode={result['input_mode']} cr={result['cr']:.4f} rules={result['applied_rules']}")
  print("✅ Restrictive profile test PASSED")


if __name__ == '__main__':
  main()
