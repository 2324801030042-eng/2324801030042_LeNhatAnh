import os
import random
from locust import HttpUser, task, between

class InsuranceUser(HttpUser):
    # Think time between requests (simulating real user interaction)
    wait_time = between(0.05, 0.15)

    def on_start(self):
        self.test_run_id = os.environ.get("TEST_RUN_ID", "RUN-DEFAULT")
        self.target_fn = int(os.environ.get("TARGET_FUNCTION", "1"))
        self.custom_headers = {
            "Content-Type": "application/json",
            "X-Test-Run-Id": self.test_run_id,
            "User-Agent": f"Locust-Agent-{self.test_run_id}"
        }

    @task
    def execute_assigned_scenario(self):
        if self.target_fn == 1:
            self.create_quote_and_calculate_premium()
        elif self.target_fn == 2:
            self.convert_quote_to_policy()
        elif self.target_fn == 3:
            self.search_quotes_and_policies()
        else:
            self.create_quote_and_calculate_premium()

    def create_quote_and_calculate_premium(self):
        """Chức năng 1 - Tạo đề nghị bảo hiểm và tính phí (Create Quote & Calculate Premium)"""
        product = random.choice(["TRI", "CAR"])
        covers = ["C1", "C2"] if product == "TRI" else ["C1", "C2", "C3"]
        payload = {
            "productCode": product,
            "policyFrom": "2026-11-23T00:00:00Z",
            "policyTo": "2027-11-23T00:00:00Z",
            "selectedCovers": covers,
            "answers": [
                {"questionCode": "NUM_OF_ADULTS", "questionType": "Choice", "answer": str(random.randint(1, 4))},
                {"questionCode": "DESTINATION", "questionType": "Choice", "answer": random.choice(["EUR", "USA", "ASIA"])}
            ]
        }
        with self.client.post("/api/offers", json=payload, headers=self.custom_headers, catch_response=True, name="Chức năng 1 - Tạo đề nghị và tính phí") as response:
            if response.status_code == 200:
                response.success()
            else:
                response.failure(f"Status code: {response.status_code}")

    def convert_quote_to_policy(self):
        """Chức năng 2 - Chuyển đề nghị thành hợp đồng (Convert Quote to Policy)"""
        offer_num = f"OFF-2026-{random.randint(100000, 999999)}"
        payload = {
            "offerNumber": offer_num,
            "policyHolder": {
                "firstName": "Nhat Anh",
                "lastName": "Le",
                "taxId": "2324801030042"
            },
            "policyHolderAddress": {
                "country": "Vietnam",
                "zipCode": "820000",
                "city": "Thu Dau Mot",
                "street": "Tran Van On"
            }
        }
        with self.client.post("/api/policies", json=payload, headers=self.custom_headers, catch_response=True, name="Chức năng 2 - Chuyển đề nghị thành hợp đồng") as response:
            if response.status_code == 200:
                response.success()
            else:
                response.failure(f"Status code: {response.status_code}")

    def search_quotes_and_policies(self):
        """Chức năng 3 - Tìm kiếm đề nghị/hợp đồng (Search Quotes & Policies)"""
        queries = ["Le Nhat Anh", "TRI", "CAR", "POL-2026", "Customer"]
        q = random.choice(queries)
        with self.client.get(f"/api/PolicySearch?q={q}", headers=self.custom_headers, catch_response=True, name="Chức năng 3 - Tìm kiếm đề nghị/hợp đồng") as response:
            if response.status_code == 200:
                response.success()
            else:
                response.failure(f"Status code: {response.status_code}")
