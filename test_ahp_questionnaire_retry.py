# -*- coding: utf-8 -*-
"""Test AHP retry re-asks only when CR meaningfully increases."""
from unittest.mock import patch
from iot_selector import IoTSelector, QUESTION_NUM_TO_KEY

# S1+S3+S5 together raise CR meaningfully vs expert base
HIGH_CR_ANSWERS = {
    'internet_nazdik': 'بله',
    'budjeh_avalieh': 'انعطاف‌پذیر',
    'hazine_amaliati': 'بسیار محدود',
    'masahat_zamin': 'بزرگ',
    'topography': 'کمی شیب‌دار',
    'manae_fiziki': 'موانع زیاد',
    'dastresi_bargh': 'عدم دسترسی',
    'pooshesh_mobile': 'پوشش ضعیف',
    'tedad_sensor': 'متوسط',
    'tarakom_sensor': 'تراکم متوسط',
    'hajm_dadeh': 'زیاد',
    'ghabeliat_gostaresh': 'اهمیت متوسط',
}


def main():
  selector = IoTSelector()
  reasked: list = []

  def track_ask(qnum, ua):
    reasked.append(qnum)
    key = QUESTION_NUM_TO_KEY[qnum]
    ua[key] = {
      'internet_nazdik': 'خیر',
      'budjeh_avalieh': 'متوسط',
      'hazine_amaliati': 'متوسط',
      'pooshesh_mobile': 'پوشش متوسط',
    }.get(key, 'متوسط')

  _, base_cr = selector._expert_baseline_ahp()
  _, first = selector._run_ahp_once(HIGH_CR_ANSWERS, source='questionnaire')
  assert first['cr'] > base_cr + 0.01, "Fixture must meaningfully inflate CR"

  with patch.object(selector, 'ask_question', side_effect=track_ask):
    with patch('builtins.input', return_value='r'):
      result = selector._personalize_ahp_from_questionnaire(HIGH_CR_ANSWERS.copy())

  assert reasked, "Retry must re-ask questions tied to inflating rules"
  assert result.get('input_mode') in (
    'expert_base_plus_rules',
    'expert_base_plus_rules_constrained',
    'expert_base_plus_rules_override',
  )
  print("reasked questions:", reasked)
  print(f"final cr={result['cr']:.4f} mode={result.get('input_mode')}")
  print("✅ Questionnaire retry test PASSED")


if __name__ == '__main__':
  main()
