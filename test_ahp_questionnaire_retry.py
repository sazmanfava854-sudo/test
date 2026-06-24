# -*- coding: utf-8 -*-
"""Test questionnaire AHP retry re-asks conflicting questions."""
from unittest.mock import patch
from iot_selector import IoTSelector, QUESTION_NUM_TO_KEY

ANSWERS = {
    'internet_nazdik': 'بله', 'budjeh_avalieh': 'انعطاف‌پذیر',
    'hazine_amaliati': 'بسیار محدود',
    'masahat_zamin': 'متوسط', 'topography': 'مسطح و هموار',
    'manae_fiziki': 'موانع متوسط', 'dastresi_bargh': 'دسترسی محدود',
    'pooshesh_mobile': 'پوشش متوسط', 'tedad_sensor': 'متوسط',
    'tarakom_sensor': 'تراکم متوسط', 'hajm_dadeh': 'متوسط',
    'ghabeliat_gostaresh': 'اهمیت متوسط',
}


def main():
  selector = IoTSelector()
  reasked: list = []

  def track_ask(qnum, ua):
    reasked.append(qnum)
    ua[QUESTION_NUM_TO_KEY[qnum]] = 'متوسط'

  high = {
    'cr': 0.35, 'applied_rules': ['S1', 'S3'],
    'cr_status': 'نامطلوب', 'rule_explanations': [], 'weights': {},
  }
  low = {
    'cr': 0.05, 'applied_rules': [],
    'cr_status': 'قابل قبول', 'rule_explanations': [], 'weights': {'هزینه': 0.2},
  }

  with patch.object(selector, 'ask_question', side_effect=track_ask):
    with patch.object(selector, '_run_ahp_once', side_effect=[
      ({'هزینه': 0.5}, high),
      ({'هزینه': 0.2}, low),
    ]):
      with patch('builtins.input', return_value='r'):
        result = selector.get_user_preferences(ANSWERS.copy())

  assert reasked, "Retry must re-ask conflicting questions"
  assert result['cr'] <= 0.10
  print("reasked questions:", reasked)
  print("✅ Questionnaire retry test PASSED")


if __name__ == '__main__':
  main()
