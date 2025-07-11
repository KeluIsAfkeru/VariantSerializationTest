# 🚀 VariantSerializationTest

## 项目简介

这是一个追求极致性能的极限序列化压缩方案，通过结合**变长编码**、**浮点量化**和**位域编码**等多种优化技术，实现了在保证高性能的同时大幅减少数据体积的目标，感谢多年前的嘿客指点呜呜。

## ✨ 核心特性

- **变长编码（Variable-Length Encoding）**：根据数据实际大小动态调整存储空间
- **浮点量化（Float Quantization）**：极限压缩浮点数精度，减少冗余信息
- **位域编码（Bit-field Encoding）**：充分利用每一个比特位，最大化存储效率，只存储实际数据杜绝位浪费

## 📊 性能表现

### 压缩效果显著

经过测试，序列化后的数据体积压缩率表现相当出色：

- **平均压缩率超过 38%**
- **最高可达 54% 的压缩效果**

<img width="1283" height="762" alt="序列化压缩率对比图" src="https://github.com/user-attachments/assets/a0337b30-5d46-401a-a443-78b4a18e6883" />

### 规模效应显著

随着数据量级的增长，压缩效果会更加显著。这意味着在处理大规模数据时，本方案能够带来更可观的存储和传输成本节省，但是总体不会超过 54%，但这样的一个压缩率也是很客观的了。

<img width="1483" height="762" alt="不同数据规模下的压缩效果" src="https://github.com/user-attachments/assets/9c1115b5-711b-471d-8b0b-2a512c5beae7" />
