#!/bin/bash

# Notion 파일명 정리 스크립트
# UUID (32자리 16진수) 제거 및 충돌 해결

# 색상 정의
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
GRAY='\033[0;37m'
NC='\033[0m' # No Color

# 기본 설정
TARGET_PATH="${1:-docs/NotionTask}"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
BACKUP_PATH="${TARGET_PATH}_backup_${TIMESTAMP}"

# 통계 변수
total_processed=0
total_skipped=0
total_errors=0

echo -e "${GREEN}🧹 Notion 파일명 정리 시작...${NC}"
echo -e "${YELLOW}📁 대상 경로: $TARGET_PATH${NC}"

# 경로 확인
if [ ! -d "$TARGET_PATH" ]; then
    echo -e "${RED}❌ 경로를 찾을 수 없습니다: $TARGET_PATH${NC}"
    echo -e "${YELLOW}현재 디렉토리: $(pwd)${NC}"
    exit 1
fi

# 백업 생성
echo -e "${CYAN}💾 백업 생성 중: $BACKUP_PATH${NC}"
if cp -r "$TARGET_PATH" "$BACKUP_PATH" 2>/dev/null; then
    echo -e "${GREEN}✅ 백업 완료!${NC}"
else
    echo -e "${YELLOW}⚠️  백업 실패${NC}"
    read -p "계속 진행하시겠습니까? (y/n): " continue_choice
    if [[ ! "$continue_choice" =~ ^[Yy]$ ]]; then
        exit 1
    fi
fi

echo -e "\n${CYAN}🔍 UUID 패턴 파일 검색 중...${NC}"

# UUID 패턴이 있는 모든 파일과 폴더를 임시 파일에 저장 (깊이 순으로 정렬)
temp_file=$(mktemp)
find "$TARGET_PATH" -name "*[0-9a-f][0-9a-f][0-9a-f][0-9a-f][0-9a-f][0-9a-f][0-9a-f][0-9a-f]*" \
    -exec bash -c 'echo "$(echo "{}" | tr "/" "\n" | wc -l) {}"' \; | \
    sort -nr | cut -d' ' -f2- > "$temp_file"

total_items=$(wc -l < "$temp_file")
echo -e "${YELLOW}📋 처리 대상: $total_items 개${NC}"

if [ "$total_items" -eq 0 ]; then
    echo -e "${GREEN}✅ UUID 패턴이 있는 파일이 없습니다!${NC}"
    rm "$temp_file"
    exit 0
fi

echo -e "\n${GREEN}🚀 파일명 정리 시작...${NC}"

# 진행률 함수
show_progress() {
    local current=$1
    local total=$2
    local percent=$((current * 100 / total))
    local filled=$((percent / 2))
    local empty=$((50 - filled))
    
    printf "\r${CYAN}진행률: [${NC}"
    printf "%${filled}s" | tr ' ' '█'
    printf "%${empty}s" | tr ' ' '░'
    printf "${CYAN}] %d%% (%d/%d)${NC}" "$percent" "$current" "$total"
}

current_item=0

# 파일 처리
while IFS= read -r file; do
    ((current_item++))
    show_progress "$current_item" "$total_items"
    
    # 경로가 존재하는지 확인 (이전 작업으로 이미 변경되었을 수 있음)
    if [ ! -e "$file" ]; then
        ((total_skipped++))
        continue
    fi
    
    dir=$(dirname "$file")
    base=$(basename "$file")
    
    # UUID 제거한 새 이름 생성
    newname=$(echo "$base" | sed 's/ [0-9a-f]\{32\}//')
    
    # 이름이 변경되지 않은 경우 (UUID가 없는 경우)
    if [ "$newname" = "$base" ]; then
        ((total_skipped++))
        continue
    fi
    
    target="$dir/$newname"
    
    # 충돌 해결
    if [ -e "$target" ]; then
        counter=2
        
        # 확장자 분리
        if [[ "$newname" == *.* ]]; then
            name_without_ext="${newname%.*}"
            ext="${newname##*.}"
            while [ -e "$dir/${name_without_ext}_${counter}.${ext}" ]; do
                ((counter++))
            done
            target="$dir/${name_without_ext}_${counter}.${ext}"
            final_name="${name_without_ext}_${counter}.${ext}"
        else
            while [ -e "$dir/${newname}_${counter}" ]; do
                ((counter++))
            done
            target="$dir/${newname}_${counter}"
            final_name="${newname}_${counter}"
        fi
    else
        final_name="$newname"
    fi
    
    # 파일/폴더 이름 변경
    if mv "$file" "$target" 2>/dev/null; then
        ((total_processed++))
        if [ "$final_name" != "$newname" ]; then
            echo -e "\n${GREEN}✅ $base → $final_name (충돌 해결)${NC}"
        else
            echo -e "\n${GREEN}✅ $base → $final_name${NC}"
        fi
    else
        ((total_errors++))
        echo -e "\n${RED}❌ 오류: $base${NC}"
    fi
    
done < "$temp_file"

# 임시 파일 정리
rm "$temp_file"

echo -e "\n\n${CYAN}📊 처리 결과:${NC}"
echo -e "  ${GREEN}✅ 성공: $total_processed 개${NC}"
echo -e "  ${GRAY}⏭️  건너뛰기: $total_skipped 개${NC}"
echo -e "  ${RED}❌ 오류: $total_errors 개${NC}"
echo -e "  ${BLUE}📦 백업: $BACKUP_PATH${NC}"

if [ "$total_errors" -gt 0 ]; then
    echo -e "\n${YELLOW}⚠️  일부 파일에서 오류가 발생했습니다.${NC}"
    echo -e "${YELLOW}오류가 발생한 파일들은 원본 상태로 유지됩니다.${NC}"
fi

echo -e "\n${GREEN}🎉 파일명 정리 완료!${NC}"

# Git 추가 제안
if [ -d ".git" ]; then
    echo -e "\n${CYAN}💡 Git 저장소가 감지되었습니다.${NC}"
    read -p "변경사항을 Git에 추가하시겠습니까? (y/n): " git_choice
    
    if [[ "$git_choice" =~ ^[Yy]$ ]]; then
        echo -e "${CYAN}📝 Git에 변경사항 추가 중...${NC}"
        git add "$TARGET_PATH"
        echo -e "${GREEN}✅ Git add 완료!${NC}"
        echo -e "${BLUE}💬 커밋 메시지 예시: git commit -m 'Clean Notion export: Remove UUIDs from filenames'${NC}"
    fi
fi

echo -e "\n${BLUE}🔧 사용법:${NC}"
echo -e "  다른 경로 처리: $0 <경로>"
echo -e "  백업 폴더: $BACKUP_PATH"
