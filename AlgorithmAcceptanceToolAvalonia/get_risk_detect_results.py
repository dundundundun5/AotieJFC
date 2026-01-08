#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
!�G�K�Ӝ���
�!�G�K�.md-��pn���U:��!�'���
"""

import re
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import seaborn as sns
from matplotlib import rcParams

# �n-�WS
plt.rcParams['font.sans-serif'] = ['SimHei', 'Microsoft YaHei', 'DejaVu Sans']
plt.rcParams['axes.unicode_minus'] = False

# �nseaborn7
sns.set_style("whitegrid")
sns.set_palette("husl")

def extract_data_from_markdown(file_path):
    """
    �markdown��-��K�pn
    """
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()

    # (ch���h<pn
    # 9M<| ~   | �Kcnp                   | 7,;p |
    pattern = r'\|\s*(\w+)\s*\|\s*(\d+)\s*\\$\rightarrow\\$\s*(\d+)\s*\|\s*(\d+)\s*\|'
    matches = re.findall(pattern, content)

    if not matches:
        print("*~0h<pn���markdown��<")
        return None

    data = []
    for match in matches:
        label = match[0]
        old_correct = int(match[1])
        new_correct = int(match[2])
        total = int(match[3])

        # ���n�
        old_accuracy = old_correct / total * 100 if total > 0 else 0
        new_accuracy = new_correct / total * 100 if total > 0 else 0

        data.append({
            '~': label,
            '�!�cnp': old_correct,
            '�!�cnp': new_correct,
            '7,;p': total,
            '�!��n�(%)': round(old_accuracy, 2),
            '�!��n�(%)': round(new_accuracy, 2),
            '�n��G(%)': round(new_accuracy - old_accuracy, 2)
        })

    return pd.DataFrame(data)

def calculate_overall_metrics(df):
    """
    ��tS�n�磊��
    """
    # 9n~{�{
    negative_labels = ['None', 'BT']  # 4'~
    positive_labels = [label for label in df['~'] if label not in negative_labels]

    # ��4'~pn
    negative_df = df[df['~'].isin(negative_labels)]
    # ��3'~pn
    positive_df = df[df['~'].isin(positive_labels)]

    # ���!�
    old_tn = negative_df['�!�cnp'].sum()  # 4'
    old_fp = negative_df['7,;p'].sum() - old_tn  # G3'
    old_tp = positive_df['�!�cnp'].sum()  # 3'
    old_fn = positive_df['7,;p'].sum() - old_tp  # G4'

    # ���!�
    new_tn = negative_df['�!�cnp'].sum()  # 4'
    new_fp = negative_df['7,;p'].sum() - new_tn  # G3'
    new_tp = positive_df['�!�cnp'].sum()  # 3'
    new_fn = positive_df['7,;p'].sum() - new_tp  # G4'

    # ;7,p
    total_samples = df['7,;p'].sum()
    negative_samples = negative_df['7,;p'].sum()
    positive_samples = positive_df['7,;p'].sum()

    # ��
    metrics = {
        '�!�': {
            '�n�(%)': round((old_tp + old_tn) / total_samples * 100, 2),
            '磊(%)': round(old_fp / negative_samples * 100, 2),
            '��(%)': round(old_fn / positive_samples * 100, 2)
        },
        '�!�': {
            '�n�(%)': round((new_tp + new_tn) / total_samples * 100, 2),
            '磊(%)': round(new_fp / negative_samples * 100, 2),
            '��(%)': round(new_fn / positive_samples * 100, 2)
        }
    }

    return metrics

def plot_label_accuracy(df):
    """
    �6�*~��n����
    """
    # �pn
    labels = df['~'].tolist()
    old_acc = df['�!��n�(%)'].tolist()
    new_acc = df['�!��n�(%)'].tolist()

    # �n�b'
    plt.figure(figsize=(14, 8))

    # �nab�Mn
    x = np.arange(len(labels))
    width = 0.35

    # �6ab�
    bars1 = plt.bar(x - width/2, old_acc, width, label='�!�', color='#FF6B6B', alpha=0.8)
    bars2 = plt.bar(x + width/2, new_acc, width, label='�!�', color='#4ECDC4', alpha=0.8)

    # ��p<~
    def autolabel(bars):
        for bar in bars:
            height = bar.get_height()
            plt.text(bar.get_x() + bar.get_width()/2., height + 0.5,
                    f'{height:.1f}%', ha='center', va='bottom', fontsize=9)

    autolabel(bars1)
    autolabel(bars2)

    # �n�h^'
    plt.xlabel('~', fontsize=12, fontweight='bold')
    plt.ylabel('�n� (%)', fontsize=12, fontweight='bold')
    plt.title('��!�(~
��n���', fontsize=14, fontweight='bold', pad=20)
    plt.xticks(x, labels, rotation=45, ha='right')
    plt.legend()
    plt.grid(True, alpha=0.3)

    # t@
    plt.tight_layout()
    plt.savefig('label_accuracy_comparison.png', dpi=300, bbox_inches='tight')
    plt.show()

    print(" �~�n����: label_accuracy_comparison.png")

def plot_overall_metrics(metrics):
    """
    �6tS���
    """
    # �pn
    metrics_names = ['�n�(%)', '磊(%)', '��(%)']
    old_values = [metrics['�!�'][m] for m in metrics_names]
    new_values = [metrics['�!�'][m] for m in metrics_names]

    # �n�b'
    plt.figure(figsize=(10, 6))

    # �nab�Mn
    x = np.arange(len(metrics_names))
    width = 0.35

    # �6ab�
    bars1 = plt.bar(x - width/2, old_values, width, label='�!�', color='#FF6B6B', alpha=0.8)
    bars2 = plt.bar(x + width/2, new_values, width, label='�!�', color='#4ECDC4', alpha=0.8)

    # ��p<~
    for i, (old_val, new_val) in enumerate(zip(old_values, new_values)):
        plt.text(i - width/2, old_val + 0.5, f'{old_val:.1f}%',
                ha='center', va='bottom', fontsize=10, fontweight='bold')
        plt.text(i + width/2, new_val + 0.5, f'{new_val:.1f}%',
                ha='center', va='bottom', fontsize=10, fontweight='bold')

        # ���G~�
        improvement = new_val - old_val
        if metrics_names[i] == '�n�(%)':
            color = 'green' if improvement > 0 else 'red'
            plt.text(i, max(old_val, new_val) + 3,
                    f'{improvement:+.1f}%', ha='center', va='bottom',
                    fontsize=9, fontweight='bold', color=color)
        else:  # 磊����N�}
            color = 'green' if improvement < 0 else 'red'
            plt.text(i, max(old_val, new_val) + 3,
                    f'{improvement:+.1f}%', ha='center', va='bottom',
                    fontsize=9, fontweight='bold', color=color)

    # �n�h^'
    plt.xlabel('', fontsize=12, fontweight='bold')
    plt.ylabel('~� (%)', fontsize=12, fontweight='bold')
    plt.title('��!�tS'���', fontsize=14, fontweight='bold', pad=20)
    plt.xticks(x, metrics_names)
    plt.legend()
    plt.grid(True, alpha=0.3)

    # ��4s��
    plt.axhline(y=0, color='black', linewidth=0.5)

    # t@
    plt.tight_layout()
    plt.savefig('overall_metrics_comparison.png', dpi=300, bbox_inches='tight')
    plt.show()

    print(" �tS���: overall_metrics_comparison.png")

def print_summary(df, metrics):
    """
    SpߡX�
    """
    print("=" * 60)
    print("!�G�K�ӜߡX�")
    print("=" * 60)

    print("\n=� ~�n�ߡ:")
    print("-" * 60)
    print(df.to_string(index=False))

    print("\n=� tS'�:")
    print("-" * 60)
    print(f"{'':<10} {'�!�':<15} {'�!�':<15} {'�G':<10}")
    print("-" * 60)

    for metric in ['�n�(%)', '磊(%)', '��(%)']:
        old_val = metrics['�!�'][metric]
        new_val = metrics['�!�'][metric]
        improvement = new_val - old_val

        # 9n{�n��G�
        if metric == '�n�(%)':
            direction = "�" if improvement > 0 else "�"
        else:  # 磊����N�}
            direction = "�" if improvement < 0 else "�"

        print(f"{metric:<10} {old_val:<15.2f} {new_val:<15.2f} {improvement:+.2f}% {direction}")

    print("\n<� s.Ѱ:")
    print("-" * 60)
    accuracy_improvement = metrics['�!�']['�n�(%)'] - metrics['�!�']['�n�(%)']
    fpr_improvement = metrics['�!�']['磊(%)'] - metrics['�!�']['磊(%)']
    fnr_improvement = metrics['�!�']['��(%)'] - metrics['�!�']['��(%)']

    print(f"1. �n��G: {accuracy_improvement:+.2f}%")
    print(f"2. 磊MN: {abs(fpr_improvement):.2f}% (9�)")
    print(f"3. ��MN: {abs(fnr_improvement):.2f}% (>W9�)")

    print("\n=� ��:")
    print("-" * 60)
    if metrics['�!�']['磊(%)'] > 5:
        print("�!�磊�	z���� et<y�")
    if metrics['�!�']['��(%)'] < 1:
        print("�!����N(�i�K:oh�")

    print("\n=� ���:")
    print(f"1. label_accuracy_comparison.png - ~�n����")
    print(f"2. overall_metrics_comparison.png - tS���")

def main():
    """
    ;�p
    """
    print("=�  ��!�G�K�pn...")

    # pn���
    markdown_file = "!�G�K�.md"

    try:
        # 1. ��pn
        print(f"=� �և�: {markdown_file}")
        df = extract_data_from_markdown(markdown_file)

        if df is None:
            print("L pn��1%����<")
            return

        print(f" ��� {len(df)} *~�pn")

        # 2. ��tS
        print("=� ��tS'�...")
        metrics = calculate_overall_metrics(df)

        # 3. ���h
        print("<� ���h...")

        # �h1: ~�n���
        plot_label_accuracy(df)

        # �h2: tS��
        plot_overall_metrics(metrics)

        # 4. SpߡX�
        print_summary(df, metrics)

        print("\n ��!")

    except FileNotFoundError:
        print(f"L ��*~0: {markdown_file}")
        print(f"�n� {markdown_file} ��X(�SM�U")
    except Exception as e:
        print(f"L �gL�: {str(e)}")
        import traceback
        traceback.print_exc()

if __name__ == "__main__":
    main()