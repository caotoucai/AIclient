#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

class QDomElement;

namespace ASAM {
namespace FIBEX {

/**
 * @brief group COMMON-LAYOUT-DETAILS
 *
 * Common attributes to describe a certain data section in a frame
 */
class ASAM_FIBEX_EXPORT FXCommonLayoutDetails
{
public:
    FXCommonLayoutDetails();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** element BIT-POSITION */
    quint16 m_bitPostion;
    /** element IS-HIGH-LOW-BYTE-ORDER */
    bool m_isBigEndian;
    /** element SEQUENCE-NUMBER */
    quint16 m_sequenceNumber;
};

} // FIBEX
} // ASAM
