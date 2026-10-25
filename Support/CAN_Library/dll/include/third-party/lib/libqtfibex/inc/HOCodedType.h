#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FibexTypes.h"

class QDomElement;

namespace ASAM {
namespace FIBEX {

class FXFibex;

/**
 * @brief element CODED-TYPE
 *
 * Coded Type of data on communication system.
 */
class ASAM_FIBEX_EXPORT HOCodedType
{
public:
    FXFibex *m_fibex = nullptr;

public:
    HOCodedType(FXFibex *fibex = nullptr);
    ~HOCodedType();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** @opt attribute ho:BASE-DATA-TYPE */
    FibexTypes::HOBaseDataType *m_baseDataType = nullptr;
    /** @opt attribute ho:BYTE-ORDER-MARK */
    /** @must attribute ho:CATEGORY */
    QString m_category;
    /** @opt attribute ho:ENCODING */
    QString m_encoding;
    /** @opt attribute ho:TERMINATION */

    /** @must element ho:BIT-LENGTH */
    quint32 m_bitLength;
    /** @must element ho:MIN-LENGTH */
    quint32 m_minLength;
    /** @must element ho:MAX-LENGTH */
    quint32 m_maxLength;

};

} // FIBEX
} // ASAM
